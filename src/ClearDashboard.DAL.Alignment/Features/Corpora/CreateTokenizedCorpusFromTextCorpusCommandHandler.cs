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

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using Token = ClearDashboard.DataAccessLayer.Models.Token;

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
            // 1. creates a new Corpus,
            // 2. creates a new associated TokenizedCorpus,
            // 3. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            //Assert.All(command.TextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            /*
             *         ITextCorpus TextCorpus, 
        bool IsRtl, 
        string Name, 
        string Language, 
        string CorpusType,
        string TokenizationQueryString
             * */
            var corpus = new Corpus
            {
                IsRtl = request.IsRtl,
                Name = request.Name,
                Language = request.Language,
            };

            if (Enum.TryParse<CorpusType>(request.CorpusType, out CorpusType corpusType))
            {
                corpus.CorpusType = corpusType;
            }

            corpus.Metadata = new Dictionary<string, object>
            {
                { "TokenizationQueryString", request.TokenizationQueryString }
            };

            var tokenizedCorpus = new TokenizedCorpus();

            foreach (var tokensTextRow in request.TextCorpus.Cast<TokensTextRow>())
            {
                tokenizedCorpus.Tokens.AddRange(tokensTextRow.Tokens.Select(engineToken => new Token
                {
                    BookNumber = engineToken.TokenId.BookNumber,
                    ChapterNumber = engineToken.TokenId.ChapterNumber,
                    VerseNumber = engineToken.TokenId.VerseNumber,
                    WordNumber = engineToken.TokenId.WordNumber,
                    SubwordNumber = engineToken.TokenId.SubWordNumber,
                    Text = engineToken.Text
                }));
            }


            ProjectDbContext.Corpa.Add(corpus);
            corpus.TokenizedCorpora.Add(tokenizedCorpus);

            await ProjectDbContext.SaveChangesAsync();


            return new RequestResult<TokenizedTextCorpus>
            (result: Task.Run(() => TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(new Guid())), cancellationToken).GetAwaiter().GetResult(),
                //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                success: true,
                message: "successful result from test");
        }
    }
}