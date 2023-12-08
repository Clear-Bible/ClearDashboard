using System;
using System.Reflection;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Builder;
using SIL.Machine.Corpora;
using System.Threading;
using ClearDashboard.DAL.Alignment.Features.Common;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using ClearDashboard.DataAccessLayer.Models;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using MediatR;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration.DifferenceModel;
using SIL.Scripture;
using SIL.Extensions;

namespace ClearDashboard.Collaboration.Merge;

public class ParallelCorpusHandler : DefaultMergeHandler<IModelSnapshot<Models.ParallelCorpus>>
{
	public ParallelCorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    public override async Task ModifyListDifferencesAsync(IListDifference<IModelSnapshot<Models.ParallelCorpus>> listDifference, IEnumerable<IModelSnapshot<Models.ParallelCorpus>>? currentSnapshotList, IEnumerable<IModelSnapshot<Models.ParallelCorpus>>? targetCommitSnapshotList, CancellationToken cancellationToken = default)
    {
        await base.ModifyListDifferencesAsync(listDifference, currentSnapshotList, targetCommitSnapshotList, cancellationToken);

        var tokenizedCorpusHandler = (TokenizedCorpusHandler)_mergeContext.FindMergeHandler<IModelSnapshot<Models.TokenizedCorpus>>();

        foreach (var parallelCorpusGuid in tokenizedCorpusHandler.VersificationChangedParallelCorpusGuids)
        {
            await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
                $"Versification changed, so replacing verse rows and verses for ParallelCorpus Id '{parallelCorpusGuid}'",
                async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                    var parallelCorpus = projectDbContext.ParallelCorpa
                        .Include(e => e.SourceTokenizedCorpus)
                        .Include(e => e.TargetTokenizedCorpus)
                        .FirstOrDefault(pc => pc.Id == parallelCorpusGuid) ?? throw new InvalidModelStateException($"Parallel corpus lookup failed for Id '{parallelCorpusGuid}', so unable to update verse mappings per tokenized corpus versification changes");

                    var previousVerseMappings = projectDbContext.VerseMappings
                        .Where(e => e.ParallelCorpusId == parallelCorpusGuid)
;
                    projectDbContext.VerseMappings.RemoveRange(previousVerseMappings);

                    var sourceTokenizedCorpus = parallelCorpus.SourceTokenizedCorpus!;
                    var targetTokenizedCorpus = parallelCorpus.TargetTokenizedCorpus!;

                    var verseMappingsForAllVerses = EngineParallelTextCorpus.VerseMappingsForAllVerses(
                        TokenizedCorpusHandler.ExtractVersification(sourceTokenizedCorpus.ScrVersType, sourceTokenizedCorpus.CustomVersData),
                        TokenizedCorpusHandler.ExtractVersification(targetTokenizedCorpus.ScrVersType, targetTokenizedCorpus.CustomVersData));

                    var newVerseMappings = verseMappingsForAllVerses
                        .Select(vm =>
                        {
                            var verseMapping = new Models.VerseMapping
                            {
                                ParallelCorpusId = parallelCorpusGuid
                            };

                            verseMapping.Verses.AddRange(ParallelCorpusDataBuilder.BuildVerses(vm.SourceVerses, parallelCorpusGuid, sourceTokenizedCorpus.CorpusId, cancellationToken));
                            verseMapping.Verses.AddRange(ParallelCorpusDataBuilder.BuildVerses(vm.TargetVerses, parallelCorpusGuid, targetTokenizedCorpus.CorpusId, cancellationToken));

                            return verseMapping;
                        });

                    parallelCorpus.VerseMappings.AddRange(newVerseMappings);

                    logger.LogInformation($"Replaced [{previousVerseMappings.Count()}] verse mappings with [{newVerseMappings.Count()}] new ones for parallel corpus '{parallelCorpusGuid}'");
                    await Task.CompletedTask;
                },
                cancellationToken
            );
        }
    }

    protected override async Task HandleCreateChildrenAsync(IModelSnapshot<Models.ParallelCorpus> parentSnapshot, CancellationToken cancellationToken)
    {
        var parallelCorpusId = (Guid)parentSnapshot.GetId();
        var sourceTokenizedCorpusId = (Guid)parentSnapshot.PropertyValues[nameof(Models.ParallelCorpus.SourceTokenizedCorpusId)]!;
        var targetTokenizedCorpusId = (Guid)parentSnapshot.PropertyValues[nameof(Models.ParallelCorpus.TargetTokenizedCorpusId)]!;
        var userId = (Guid)parentSnapshot.PropertyValues[nameof(Models.ParallelCorpus.UserId)]!;
        var displayName = (string?)parentSnapshot.PropertyValues[nameof(Models.ParallelCorpus.DisplayName)];

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Inserting ParallelCorpus named '{displayName}'",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var sourceTokenizedCorpus = projectDbContext.TokenizedCorpora.First(tc => tc.Id == sourceTokenizedCorpusId);
                var targetTokenizedCorpus = projectDbContext.TokenizedCorpora.First(tc => tc.Id == targetTokenizedCorpusId);

                var sourceVersification = TokenizedCorpusHandler.ExtractVersification(sourceTokenizedCorpus.ScrVersType, sourceTokenizedCorpus.CustomVersData);
                var targetVersification = TokenizedCorpusHandler.ExtractVersification(targetTokenizedCorpus.ScrVersType, targetTokenizedCorpus.CustomVersData);

                var sourceVerseRowsForTokenization = projectDbContext.VerseRows
                    .Where(e => e.TokenizedCorpusId == sourceTokenizedCorpusId)
                    .OrderBy(e => e.BookChapterVerse)
                    .ToList()
                    .Select(e => (e.BookChapterVerse!, e.OriginalText ?? string.Empty, e.IsSentenceStart));
                var targetVerseRowsForTokenization = projectDbContext.VerseRows
                    .Where(e => e.TokenizedCorpusId == targetTokenizedCorpusId)
                    .OrderBy(e => e.BookChapterVerse)
                    .ToList()
                    .Select(e => (e.BookChapterVerse!, e.OriginalText ?? string.Empty, e.IsSentenceStart));

                var sourceTokenizedTextCorpus = TokenizedCorpusHandler.ExtractITextCorpus(
                    sourceTokenizedCorpusId,
                    sourceTokenizedCorpus.TokenizationFunction,
                    sourceVersification,
                    sourceVerseRowsForTokenization
                    );
                var targetTokenizedTextCorpus = TokenizedCorpusHandler.ExtractITextCorpus(
                    targetTokenizedCorpusId,
                    targetTokenizedCorpus.TokenizationFunction,
                    targetVersification,
                    targetVerseRowsForTokenization
                    );
                //var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(_mergeContext.Mediator, new TokenizedTextCorpusId(sourceTokenizedCorpusId));
                //var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(_mergeContext.Mediator, new TokenizedTextCorpusId(targetTokenizedCorpusId));
                var verseMappingsForAllVerses = EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceVersification,
                    targetVersification);

                progress.Report(new ProgressStatus(0, $"Starting EngineAlignRows for Parallel Corpus '{displayName}'"));
                logger.LogInformation($"Starting EngineAlignRows for Parallel Corpus '{displayName}'");
                var engineParallelTextCorpus =
                    await Task.Run(() => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                        new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsForAllVerses)), cancellationToken);
                progress.Report(new ProgressStatus(0, $"Completed EngineAlignRows for Parallel Corpus '{displayName}'"));
                logger.LogInformation($"Completed EngineAlignRows for Parallel Corpus '{displayName}'");

                var parallelCorpusModel = ParallelCorpusDataBuilder.BuildParallelCorpus(
                    parallelCorpusId,
                    sourceTokenizedCorpus,
                    targetTokenizedCorpus,
                    verseMappingsForAllVerses,
                    displayName,
                    cancellationToken);

                // Use the original parallel corpus user id clear down to the
                // verse level:
                foreach (var vm in parallelCorpusModel.VerseMappings)
                {
                    vm.UserId = userId;
                    foreach (var v in vm.Verses)
                    {
                        v.UserId = userId;
                    }
                }

                projectDbContext.VerseMappings.AddRange(parallelCorpusModel.VerseMappings);

                progress.Report(new ProgressStatus(0, $"Inserted {verseMappingsForAllVerses.Count()} verse mappings along with parallel corpus '{displayName}'"));
                logger.LogInformation($"Inserted {verseMappingsForAllVerses.Count()} verse mappings along with parallel corpus '{displayName}'");
            },
            cancellationToken
        );

        await base.HandleCreateChildrenAsync(parentSnapshot, cancellationToken);
    }
}

