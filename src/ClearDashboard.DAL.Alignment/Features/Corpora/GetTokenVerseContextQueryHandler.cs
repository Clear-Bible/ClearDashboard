using System.Collections.Generic;
using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTokenVerseContextQueryHandler : ProjectDbContextQueryHandler<
        GetTokenVerseContextQuery,
        RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>,
        (IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>
    {

        public GetTokenVerseContextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTokenVerseContextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>> 
            GetDataAsync(GetTokenVerseContextQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            (IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex) tokenVerseContext = default;

            if (request.ParallelCorpusId is null)
            {
                tokenVerseContext = TokenVerseContextFinder.GetTokenVerseRowContext(
                    request.Token, 
                    ProjectDbContext, 
                    Logger);
            }
            else
            {
                var parallelCorpus = ProjectDbContext!.ParallelCorpa
                    .Include(e => e!.SourceTokenizedCorpus)
                    .Include(e => e!.TargetTokenizedCorpus)
                    .Where(e => e.Id == request.ParallelCorpusId.Id)
                    .FirstOrDefault();

                if (parallelCorpus == null)
                {
                    return new RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>
                    (
                        success: false,
                        message: $"ParallelCorpus not found for ParallelCorpusId '{request.ParallelCorpusId.Id}'"
                    );
                }

                var tokenComponent = ProjectDbContext!.TokenComponents
                    .Where(e => e.Id == request.Token.TokenId.Id)
                    .FirstOrDefault();

                if (tokenComponent == null)
                {
                    return new RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>
                    (
                        success: false,
                        message: $"TokenComponent not found for TokenComponent Id '{request.Token.TokenId.Id}'"
                    );
                }

                if (tokenComponent.TokenizedCorpusId != parallelCorpus.SourceTokenizedCorpusId &&
                    tokenComponent.TokenizedCorpusId != parallelCorpus.TargetTokenizedCorpusId)
                {
                    return new RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>
                    (
                        success: false,
                        message: $"TokenComponent TokenizedCorpus Id '{tokenComponent.TokenizedCorpusId}' not part of ParallelCorpus Id '{parallelCorpus.Id}'"
                    );
                }

                var tokenVerseContexts = TokenVerseContextFinder.GetTokenVerseContexts(
                    parallelCorpus,
                    new List<Token> { request.Token },
                    tokenComponent.TokenizedCorpusId == parallelCorpus.SourceTokenizedCorpusId,
                    ProjectDbContext, 
                    Logger);

                if (!tokenVerseContexts.TryGetValue(request.Token.TokenId, out tokenVerseContext))
                {
                    return new RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>
                    (
                        success: false,
                        message: $"No verse context found for Token Id '{tokenComponent.Id}' and ParallelCorpus Id '{parallelCorpus.Id}'"
                    );
                }
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            await Task.CompletedTask;
            return new RequestResult<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>(tokenVerseContext);
        }
    }
}
