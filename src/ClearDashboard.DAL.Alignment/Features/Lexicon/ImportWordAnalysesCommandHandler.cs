using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;

//USE TO ACCESS Vocabulary
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class ImportWordAnalysesCommandHandler : ProjectDbContextCommandHandler<ImportWordAnalysesCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IUserProvider _userProvider;
		private readonly IMediator _mediator;
		public ImportWordAnalysesCommandHandler(
            IMediator mediator,
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            IUserProvider userProvider,
            ILogger<ImportWordAnalysesCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
            _userProvider = userProvider;
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(ImportWordAnalysesCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = ModelHelper.BuildUserId(_userProvider.CurrentUser!);
            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

			var sw = Stopwatch.StartNew();

            try
            {
				using var splitTokenDbCommands = await SplitTokenDbCommands.CreateAsync(ProjectDbContext, _userProvider, Logger, cancellationToken);

				// Query matching no-association tokens having matching TokenizedCorpusId
				var supersetTokens = await ProjectDbContext.Tokens
					.Include(t => t.TokenCompositeTokenAssociations)
					.Include(t => t.TokenizedCorpus)
					.ThenInclude(tc => tc!.SourceParallelCorpora)
					.Include(t => t.TokenizedCorpus)
					.ThenInclude(tc => tc!.TargetParallelCorpora)
					.Include(t => t.Translations.Where(a => a.Deleted == null))
					.Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
					.Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
					.Where(t => t.Deleted == null)
                    .Where(t => t.TokenCompositeTokenAssociations.Count == 0)
					.ToListAsync(cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var previouslySplitTokensById = await ProjectDbContext.Tokens
                    .Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
                    .Where(t => t.Deleted != null)
                    .Where(t => t.Metadata.Any(m => m.Key == Models.MetadatumKeys.WasSplit && m.Value == true.ToString()))
                    .ToDictionaryAsync(t => t.Id, t => t, cancellationToken: cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				var compositesFormedViaWordAnalysisImport = await ProjectDbContext.TokenComposites
					.Include(t => t.TokenCompositeTokenAssociations)
                        .ThenInclude(t => t.Token)
					.Include(t => t.Translations.Where(a => a.Deleted == null))
					.Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
					.Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
                    .Where(t => t.Deleted == null)
					.Where(t => t.Metadata.Any(m => m.Key == Models.MetadatumKeys.SplitTokenSourceId && m.Value != null))
                    .ToListAsync(cancellationToken: cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				var unchangedSplitComposites = new List<(Guid SourceId, string SourceSurfaceText, string SplitMatchInfoAsHash, Models.TokenComposite TokenComposite)>();
                foreach (var tokenComposite in compositesFormedViaWordAnalysisImport)
                {
					cancellationToken.ThrowIfCancellationRequested();

					var sourceId = Guid.Parse(tokenComposite.Metadata.Single(e => e.Key == Models.MetadatumKeys.SplitTokenSourceId).Value!);
					var sourceSurfaceText = tokenComposite.Metadata.SingleOrDefault(e => e.Key == Models.MetadatumKeys.SplitTokenSourceSurfaceText)?.Value;
					var initialSplitMatchInfo = tokenComposite.Metadata.SingleOrDefault(e => e.Key == Models.MetadatumKeys.SplitTokenInitialChildren)?.Value;
                    if (sourceSurfaceText is null || initialSplitMatchInfo is null)
                    {
                        Logger.LogInformation($"Token composite '{tokenComposite.Id}' has a {nameof(Models.MetadatumKeys.SplitTokenSourceId)} but is missing either {nameof(Models.MetadatumKeys.SplitTokenSourceSurfaceText)} or {nameof(Models.MetadatumKeys.SplitTokenInitialChildren)} from Metadatum...skipping");
                        continue;
                    }
                    if (initialSplitMatchInfo == tokenComposite.GetSplitMatchInfoAsHash())
                    {
                        unchangedSplitComposites.Add((sourceId, sourceSurfaceText, initialSplitMatchInfo, tokenComposite));
                    }
                    else
                    {
                        Logger.LogWarning($"Token composite '{tokenComposite.Id}' intial split match info does not match {tokenComposite.GetSplitMatchInfoAsHash()} {string.Join(",", tokenComposite.Tokens.Select(t => t.Type))}");
                    }
                }

                Logger.LogInformation($"Superset token count: {supersetTokens.Count}");
				Logger.LogInformation($"Previously split token count: {previouslySplitTokensById.Count}");
				Logger.LogInformation($"Unchanged, previously split-formed composite count: {unchangedSplitComposites.Count}");
				Logger.LogInformation($"Word analysis count: {request.WordAnalyses.Count()}");

				// For each word
				var waCount = 0;
                var childTokensCreatedCount = 0;
                var compositeTokensCreatedCount = 0;

                foreach (var wordAnalysis in request.WordAnalyses)
                {
					cancellationToken.ThrowIfCancellationRequested();

					Logger.LogInformation($"WordAnalysis for {wordAnalysis.Word}:");

                    //var splitInstructions = ToSplitInstructions(wordAnalysis, Logger);
                    CheckWordAnalysis(wordAnalysis, Logger);

                    var replacementTokenInfos = wordAnalysis.Lexemes
                        .Select(e => (surfaceText: e.Lemma!, trainingText: e.Lemma!, tokenType: e.Type, circumfixGroup: default(string?), grammarId: default(Guid?)))
                        .ToArray();

                    // If there are no Lexemes for this word analysis, there is nothing to split the token into, so skip:
                    if (replacementTokenInfos.Length == 0)
                    {
                        continue;
                    }

                    var standaloneTokens = supersetTokens.Where(e => e.SurfaceText == wordAnalysis.Word).ToList();

                    if (replacementTokenInfos.Length == 1)
                    {
                        if (replacementTokenInfos[0].surfaceText != wordAnalysis.Word)
                        {
                            Logger.LogError($"\tWordAnalysis for {wordAnalysis.Word} results in single replacement word: {replacementTokenInfos[0].surfaceText} of type {replacementTokenInfos[0].tokenType}.  What is supposed to happen here?");
                            continue;
                        }

                        // Using WordAnalysis to specify TokenType - no splitting here
                        foreach (var t in standaloneTokens)
                        {
                            t.Type = replacementTokenInfos[0].tokenType;
                            splitTokenDbCommands.AddTokenComponentToUpdateType(t);
                        }
                        waCount++;
                        continue;
                    }
                    else if (replacementTokenInfos.Count() > 2)
                    {
                        Logger.LogInformation($"\tWord anaylsis for {wordAnalysis.Word} breaks up word into {replacementTokenInfos.Count()} parts");
                    }

                    Stopwatch? frequentWordStopwatch = null;
                    if (standaloneTokens.Count > 500)
                    {
                        Logger.LogInformation($"\tLarge number of Token matches for word {wordAnalysis.Word}:  {standaloneTokens.Count}");
                        frequentWordStopwatch = Stopwatch.StartNew();
                    }

                    var (
                            splitCompositeTokensByIncomingTokenId,
                            splitChildTokensByIncomingTokenId
                        ) = SplitTokenUtil.SplitTokensIntoReplacements(
                        standaloneTokens,
                        replacementTokenInfos,
                        false, // CreateParallelComposite
                        splitTokenDbCommands,
                        cancellationToken
                    );

                    // Select from previous WordAnalysis import composites where its source surface text matches the current word
                    // and its child token 'split match info' hash is different from the incoming one (meaning it needs to be resplit)
                    var wordAnalysisLexemeHash = replacementTokenInfos.GetSplitMatchInfoAsHash();
                    var tokensToResplit = new Dictionary<Guid, (Models.Token OriginalToken, Models.TokenComposite TokenComposite)>();
                    foreach (var tc in unchangedSplitComposites
                        .Where(e => e.SourceSurfaceText == wordAnalysis.Word)
                        .Where(e => e.SplitMatchInfoAsHash != wordAnalysisLexemeHash)
                        .Select(e => (e.SourceId, e.TokenComposite)))
                    {
                        if (!previouslySplitTokensById.TryGetValue(tc.SourceId, out var previousSplitTokenSource))
                        {
                            Logger.LogWarning($"Composite token '{tc.TokenComposite.Id}' has a split source id of '{tc.SourceId}' that does not exist in previously split tokens list.  Skipping...");
                            continue;
                        }

                        tokensToResplit.Add(tc.SourceId, (previousSplitTokenSource, tc.TokenComposite));
                    }

                    if (tokensToResplit.Count != 0)
                    {
                        Logger.LogInformation($"\tWord anaylsis for {wordAnalysis.Word} includes re-splitting {tokensToResplit.Count} tokens");

                        var replacementsBySourceId =
                            SplitTokenUtil.CreateTokenReplacements(
                                tokensToResplit.Select(e => e.Value.OriginalToken).ToList(),
                                replacementTokenInfos,
                                splitTokenDbCommands,
                                cancellationToken
                            );

                        foreach (var kvp in tokensToResplit)
                        {
							cancellationToken.ThrowIfCancellationRequested();

							if (replacementsBySourceId.TryGetValue(kvp.Key, out var replacements))
                            {
                                var previousTrainingText = kvp.Value.TokenComposite.TrainingText;

                                splitTokenDbCommands.AddTokenCompositeChildrenToDelete(kvp.Value.TokenComposite);

                                var replacedComposite = SplitTokenUtil.AssignChildTokensToTokenComposite(
                                    kvp.Value.TokenComposite,
                                    replacements.ReplacementTokens,
                                    splitTokenDbCommands);
                                splitTokenDbCommands.AddAlignmentTrainingTextChange(kvp.Value.TokenComposite.SourceAlignments, previousTrainingText, replacedComposite.TrainingText);
                            }
                        }
                    }

                    // Since first-time splits transfer note associations from the original token to its replacement composite(s)
                    // and we are reusing those replacement composite(s), there shouldn't be anything we need to do here.  

					if (standaloneTokens.Count > 500)
					{
                        frequentWordStopwatch?.Stop();
						Logger.LogInformation($"\tElapsed time for word {wordAnalysis.Word} with large number of Token matches:  {frequentWordStopwatch?.Elapsed}");
					}

					waCount++;
                    childTokensCreatedCount += splitChildTokensByIncomingTokenId.Sum(e => e.Value.Count());
                    compositeTokensCreatedCount += splitCompositeTokensByIncomingTokenId.Sum(e => e.Value.Count());

                    if (waCount % 30 == 0 && waCount != 0)
                    {
                        Logger.LogInformation($"\tWord analyses processed: {waCount} during elapsed time {sw.Elapsed}.  Child tokens created: {childTokensCreatedCount}, composite tokens created: {compositeTokensCreatedCount}");
                    }
				}

                await splitTokenDbCommands.ExecuteBulkOperationsAsync(cancellationToken);
				await splitTokenDbCommands.CommitTransactionAsync(_mediator, cancellationToken);

				Logger.LogInformation($"Total word analyses processed: {waCount} during elapsed time {sw.Elapsed}.  Child tokens created: {childTokensCreatedCount}, composite tokens created: {compositeTokensCreatedCount}");

				sw.Stop();
                Logger.LogInformation($"Time elapsed:  {sw.Elapsed} (log)");
                Debug.WriteLine($"Time elapsed:  {sw.Elapsed} (debug)");
                Console.WriteLine($"Time elapsed:  {sw.Elapsed} (console)");

				return new RequestResult<Unit>(Unit.Value);
			}
			catch (OperationCanceledException)
            {
				return new RequestResult<Unit>
				{
					Message = "Operation Canceled",
					Success = false,
					Canceled = true
				};
			}
			catch (Exception ex)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: ex.Message
                );
            }
		}

		private static bool CheckWordAnalysis(Alignment.Lexicon.WordAnalysis wordAnalysis, ILogger logger)
        {
            var wordAnalysisPartsAsWord = string.Join("", wordAnalysis.Lexemes.Select(e => e.Lemma));
            if (wordAnalysisPartsAsWord != wordAnalysis.Word)
            {
				logger.LogWarning($"WordAnalysis parts {wordAnalysisPartsAsWord} vary from source word {wordAnalysis.Word}");
                return false;
			}

            return true;
		}
    }
}