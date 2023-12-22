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
using SIL.Machine.Utils;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Merge;

public class AlignmentSetHandler : DefaultMergeHandler<IModelSnapshot<Models.AlignmentSet>>
{
	public AlignmentSetHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    public static (Type EntityType, string EntityId, string ItemName) AlignmentSetCacheKey(Guid alignmentSetId) => 
        (typeof(Models.AlignmentSet), alignmentSetId.ToString()!, nameof(Models.AlignmentSet));

    public static (Type EntityType, string EntityId, string ItemName) DenormalizationTrainingTextCacheKey(Guid sourceTokenizedCorpusId) =>
        (typeof(Models.AlignmentSetDenormalizationTask), sourceTokenizedCorpusId!.ToString()!, nameof(Models.TokenComponent.EngineTokenId));

    protected override async Task HandleCreateChildrenAsync(IModelSnapshot<Models.AlignmentSet> parentSnapshot, CancellationToken cancellationToken)
    {
        var alignmentSetId = (Guid)parentSnapshot.GetId();

        _mergeContext.Logger.LogInformation("Loading tokenized corpora token locations into cache for creating Alignments");

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Loading token locations into cache",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var alignmentSet = projectDbContext.AlignmentSets
                    .Include(e => e.ParallelCorpus)
                    .Where(e => e.Id == alignmentSetId)
                    .FirstOrDefault();

                if (alignmentSet is null)
                    throw new InvalidModelStateException($"No alignment set found for Id '{alignmentSetId}' when about to create alignment children");

                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, alignmentSet.ParallelCorpus!.SourceTokenizedCorpusId, cache);
                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, alignmentSet.ParallelCorpus!.TargetTokenizedCorpusId, cache);

                await Task.CompletedTask;
            },
            cancellationToken);

        _mergeContext.Progress.Report(new ProgressStatus(0, "Creating Alignments"));
        _mergeContext.Logger.LogInformation("Starting create Alignments");

//       await base.HandleCreateChildrenAsync(parentSnapshot, cancellationToken);
// OR:

        var count = 0;
        var alignmentsChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.Alignment)].childName;
        if (parentSnapshot.Children.ContainsKey(alignmentsChildName))
        {
            var firstChild = parentSnapshot.Children[alignmentsChildName].FirstOrDefault();
            if (firstChild is not null)
            {
                var firstAlignment = (IModelSnapshot)firstChild;
                var handler = _mergeContext.FindMergeHandler<IModelSnapshot<Models.Alignment>>();

                _mergeContext.MergeBehavior.StartInsertModelCommand(firstAlignment);
                foreach (var child in parentSnapshot.Children[alignmentsChildName])
                {
                    var id = await _mergeContext.MergeBehavior.RunInsertModelCommand((IModelSnapshot)child, cancellationToken);

                    var sourceTokenLocation = (string)child.PropertyValues[AlignmentBuilder.SOURCE_TOKEN_LOCATION]!;
                    count++;
                }
                _mergeContext.MergeBehavior.CompleteInsertModelCommand(firstAlignment.EntityType);
            }
        }

        _mergeContext.Progress.Report(new ProgressStatus(0, $"Completed Creating Alignments (count: {count})"));
        _mergeContext.Logger.LogInformation($"Completed create Alignments (count: {count})");
    }
}

