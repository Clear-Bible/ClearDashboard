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

            var tokenizedCorpus = new TokenizedCorpus
            {
                CorpusId = request.CorpusId.Id,
                TokenizationFunction = request.TokenizationFunction
            };
            
            tokenizedCorpus.Tokens.AddRange(request.TextCorpus.Cast<TokensTextRow>()
                .SelectMany(tokensTextRow => tokensTextRow.Tokens
                    .Select(token => new Models.Token
                    {
                        BookNumber = token.TokenId.BookNumber,
                        ChapterNumber = token.TokenId.ChapterNumber,
                        VerseNumber = token.TokenId.VerseNumber,
                        WordNumber = token.TokenId.WordNumber,
                        SubwordNumber = token.TokenId.SubWordNumber,
                        SurfaceText = token.Text
                    })
                ));

            ProjectDbContext.TokenizedCorpora.Add(tokenizedCorpus);

            // NB:  passing in the cancellation token to SaveChangesAsync.
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(tokenizedCorpus.Id));

            return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
        }
    }
}