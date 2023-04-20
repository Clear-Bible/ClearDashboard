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

namespace ClearDashboard.Collaboration.Merge;

public class ParallelCorpusHandler : DefaultMergeHandler
{
	public ParallelCorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    protected override async Task CreateChildrenAsync<T>(T parentSnapshot, CancellationToken cancellationToken)
    {
        if (!typeof(T).IsAssignableTo(typeof(IModelSnapshot<Models.ParallelCorpus>)))
        {
            throw new NotImplementedException($"Derived merge handler with '{typeof(T).ShortDisplayName()}' model-specific HandleModifyProperties functionality");
        }

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
    }
}

