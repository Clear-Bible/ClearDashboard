using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using EFCore.BulkExtensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateTokenizedCorpusFromTextCorpusCommandHandler : ProjectDbContextCommandHandler<
        CreateTokenizedCorpusFromTextCorpusCommand,
        RequestResult<TokenizedTextCorpus>,
        TokenizedTextCorpus>
    {
        private readonly IMediator _mediator;

        public CreateTokenizedCorpusFromTextCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTokenizedCorpusFromTextCorpusCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<TokenizedTextCorpus>> SaveDataAsync(
            CreateTokenizedCorpusFromTextCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            // 1. creates a new associated TokenizedCorpus (associated with the parent CorpusId provided in the request),
            // 2. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.

            var corpus = ProjectDbContext!.Corpa.FirstOrDefault(c => c.Id == request.CorpusId.Id);
            if (corpus == null)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Invalid CorpusId '{request.CorpusId.Id}' found in request"
                );
            }

            var tokenizedCorpus = new TokenizedCorpus
            {
                Corpus = corpus,
                TokenizationFunction = request.TokenizationFunction
            };

            ProjectDbContext.TokenizedCorpora.Add(tokenizedCorpus);

            await ProjectDbContext.SaveChangesAsync(cancellationToken);

            //tokenizedCorpus.Tokens.AddRange(request.TextCorpus.Cast<TokensTextRow>()
            //    .SelectMany(tokensTextRow => tokensTextRow.Tokens
            //        .Select(token => new Models.Token
            //        {
            //            BookNumber = token.TokenId.BookNumber,
            //            ChapterNumber = token.TokenId.ChapterNumber,
            //            VerseNumber = token.TokenId.VerseNumber,
            //            WordNumber = token.TokenId.WordNumber,
            //            SubwordNumber = token.TokenId.SubWordNumber,
            //            SurfaceText = token.SurfaceText,
            //            TrainingText = token.TrainingText
            //        })
            //    ));


           
            var tokens = request.TextCorpus.Cast<TokensTextRow>()
                .SelectMany(tokensTextRow => tokensTextRow.Tokens
                    .Select(token => new Models.Token
                    {
                        Id = Guid.NewGuid(),
                        TokenizationId = tokenizedCorpus.Id,
                        BookNumber = token.TokenId.BookNumber,
                        ChapterNumber = token.TokenId.ChapterNumber,
                        VerseNumber = token.TokenId.VerseNumber,
                        WordNumber = token.TokenId.WordNumber,
                        SubwordNumber = token.TokenId.SubWordNumber,
                        SurfaceText = token.SurfaceText,
                        TrainingText = token.TrainingText
                    })
                ).ToList();

            var bookNumbers = tokens.GroupBy(token => token.BookNumber).Select(g => g.Key);
            var bookNumbersToAbbreviations =
                ClearBible.Engine.Persistence.FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                    x => x.silCannonBookAbbrev);

            var bookAbbreviations = new List<string>();

            foreach (var bookNumber in bookNumbers)
            {
                if (!bookNumbersToAbbreviations.TryGetValue(bookNumber, out string? bookAbbreviation))
                {
                    return new RequestResult<TokenizedTextCorpus>
                    (
                        success: false,
                        message: $"Book number '{bookNumber}' not found in FileGetBooks.BookIds"
                    );
                }

                bookAbbreviations.Add(bookAbbreviation);
            }

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now}: About to BulkInsertAsync");
            await ProjectDbContext.BulkInsertAsync(tokens, cancellationToken: cancellationToken);
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now}: Completed BulkInsertAsync.  About to Get");

            //            await ProjectDbContext.SaveChangesAsync(cancellationToken);
//            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(tokenizedCorpus.Id));
            var tokenizedTextCorpus =  new TokenizedTextCorpus(
                new TokenizedCorpusId(tokenizedCorpus.Id),
                request.CorpusId, 
                _mediator, 
                bookAbbreviations);

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now}: Completed Get");

            return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
        }
    }
}