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

            // ITextCorpus Text ids always book ids/abbreviations:  
            var bookIds = request.TextCorpus.Texts.Select(t => t.Id).ToList();

//            using var transaction = ProjectDbContext.Database.BeginTransaction();
            try
            {
                ProjectDbContext.TokenizedCorpora.Add(tokenizedCorpus);

                await ProjectDbContext.SaveChangesAsync(cancellationToken);

                // Bulk insert at a book granularity, and within each book 
                // order by chapter and verse number:
                foreach (var bookId in bookIds)
                {
                    var chapterTokens = request.TextCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>()
                        .SelectMany(ttr => ttr.Tokens)
                        .OrderBy(t => t.TokenId.ChapterNumber)
                        .ThenBy(t => t.TokenId.VerseNumber)
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
                        });

                    await ProjectDbContext.BulkInsertAsync(chapterTokens.ToList(), cancellationToken: cancellationToken);
                }

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
//                transaction.Commit();
            }
            catch (Exception ex)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Error saving tokenized corpus / tokens to database '{ex.Message}'"
                );
            }

//            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(tokenizedCorpus.Id));
            var tokenizedTextCorpus = new TokenizedTextCorpus(
                new TokenizedCorpusId(tokenizedCorpus.Id),
                request.CorpusId,
                _mediator,
                bookIds);

            return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
        }
    }
}