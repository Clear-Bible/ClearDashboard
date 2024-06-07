using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

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

			var sourceTrainingTextsByAlignmentSetId = new Dictionary<Guid, List<string>>();

			var sw = Stopwatch.StartNew();

            try
            {
				using var splitTokenDbCommands = await SplitTokenDbCommands.CreateAsync(ProjectDbContext, _userProvider, cancellationToken);

				// Query matching no-association tokens having matching TokenizedCorpusId
				var supersetTokens = ProjectDbContext.Tokens
					.Include(t => t.TokenCompositeTokenAssociations)
					.Include(t => t.TokenizedCorpus)
					.ThenInclude(tc => tc!.SourceParallelCorpora)
					.Include(t => t.TokenizedCorpus)
					.ThenInclude(tc => tc!.TargetParallelCorpora)
					.Include(t => t.Translations.Where(a => a.Deleted == null))
					.Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
					.Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
					.Where(e => e.TokenCompositeTokenAssociations.Count == 0)
					.Where(e => e.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
					.ToList();

                Logger.LogInformation($"Superset token count: {supersetTokens.Count}");
				Logger.LogInformation($"Word analysis count: {request.WordAnalyses.Count()}");

				// For each word
				var waCount = 0;
                var childTokensCreatedCount = 0;
                var compositeTokensCreatedCount = 0;

				foreach (var wordAnalysis in request.WordAnalyses)
                {
                    Logger.LogInformation($"WordAnalysis for {wordAnalysis.Word}:");

					//var splitInstructions = ToSplitInstructions(wordAnalysis, Logger);
                    CheckWordAnalysis(wordAnalysis, Logger);

					var replacementTokenInfos = wordAnalysis.Lexemes
                        .Select(e => (surfaceText: e.Lemma!, trainingText: e.Lemma!, tokenType: e.Type))
                        .ToArray();

                    var standaloneTokens = supersetTokens.Where(e => e.SurfaceText == wordAnalysis.Word).ToList();

                    if (replacementTokenInfos.Count() == 1)
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
							splitChildTokensByIncomingTokenId,
							denormalizationDataForWord
						) = SplitTokensViaSplitInstructionsCommandHandler.SplitTokensIntoReplacements(
						standaloneTokens,
						replacementTokenInfos,
						false, // CreateParallelComposite
						splitTokenDbCommands,
						cancellationToken
					);

                    foreach (var kvp in denormalizationDataForWord)
                    {
                        if (sourceTrainingTextsByAlignmentSetId.TryGetValue(kvp.Key, out var alignmentSetTexts))
                        {
                            alignmentSetTexts.AddRange(kvp.Value);
                        }
                        else
                        {
                            sourceTrainingTextsByAlignmentSetId.Add(kvp.Key, kvp.Value);
                        }
                    }

                    // Query composites by TokenizedCorpusId and ParallelCorpusId (which may be null) that have a SPLIT_SOURCE
                    // For each, check its SPLIT_INITIAL...value against split instructions, to see if it needs to be re-split

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

				if (sourceTrainingTextsByAlignmentSetId.Count != 0)
                {
					using (var denormalizationTaskInsertCommand = AlignmentUtil.CreateAlignmentDenormalizationTaskInsertCommand(splitTokenDbCommands.Connection))
                    {
                        foreach (var kvp in sourceTrainingTextsByAlignmentSetId)
                        {
                            var sourceTexts = kvp.Value.Distinct().ToList();
                            if (sourceTexts.Count <= 20)
                            {
                                foreach (var sourceText in sourceTexts)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    await AlignmentUtil.InsertAlignmentDenormalizationTaskAsync(new Models.AlignmentSetDenormalizationTask
                                    {
                                        Id = Guid.NewGuid(),
                                        AlignmentSetId = kvp.Key,
                                        SourceText = sourceText,
                                    }, denormalizationTaskInsertCommand, cancellationToken);
                                }
                            }
                            else
                            {
								cancellationToken.ThrowIfCancellationRequested();

								await AlignmentUtil.InsertAlignmentDenormalizationTaskAsync(new Models.AlignmentSetDenormalizationTask
								{
									Id = Guid.NewGuid(),
									AlignmentSetId = kvp.Key,
									SourceText = null,
								}, denormalizationTaskInsertCommand, cancellationToken);
							}
						}
                    }
                }

				await splitTokenDbCommands.CommitTransactionAsync(cancellationToken);

				Logger.LogInformation($"Total word analyses processed: {waCount} during elapsed time {sw.Elapsed}.  Child tokens created: {childTokensCreatedCount}, composite tokens created: {compositeTokensCreatedCount}");

				sw.Stop();
                Logger.LogInformation($"Time elapsed:  {sw.Elapsed} (log)");
                Debug.WriteLine($"Time elapsed:  {sw.Elapsed} (debug)");
                Console.WriteLine($"Time elapsed:  {sw.Elapsed} (console)");
            }
            catch (Exception ex)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: ex.Message
                );
            }

            if (sourceTrainingTextsByAlignmentSetId.Count != 0)
            {
				Logger.LogInformation($"Firing alignment data denormalization event");
				await _mediator.Publish(new AlignmentSetSourceTrainingTextsUpdatedEvent(sourceTrainingTextsByAlignmentSetId), cancellationToken);
			}

			return new RequestResult<Unit>();
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

		private static SplitInstructions? ToSplitInstructions(Alignment.Lexicon.WordAnalysis wordAnalysis, ILogger logger)
        {
            var trainingTexts = new List<string?>();
            var splitIndexes = new List<int>();
            foreach (var (type, lemma) in wordAnalysis.Lexemes.Select(e => (e.Type, e.Lemma)))
            {
                if (string.IsNullOrEmpty(lemma))
                {
                    logger.LogWarning($"WordAnalysis of {wordAnalysis.Word} contains a null or empty lemma");
                    continue;
                }

                var startingIndex = splitIndexes.LastOrDefault(0);
                var wordPart = wordAnalysis.Word.Substring(startingIndex, lemma.Length);
                if (wordPart != lemma)
                {
                    logger.LogError($"WordAnalysis of {wordAnalysis.Word} starting at index {startingIndex} has value of {wordPart}, which does not match lemma {lemma}.  Ignoring and moving on to next WordAnalysis...");
                    return null;
                }

                splitIndexes.Add(startingIndex + lemma.Length);
                trainingTexts.Add(lemma);
            }

            return SplitInstructions.CreateSplits(wordAnalysis.Word, splitIndexes, trainingTexts);
        }
    }
}