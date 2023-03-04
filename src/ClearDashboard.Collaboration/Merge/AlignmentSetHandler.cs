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

namespace ClearDashboard.Collaboration.Merge;

public class AlignmentSetHandler : DefaultMergeHandler
{
	public AlignmentSetHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    protected override async Task CreateChildrenAsync<T>(T parentSnapshot, CancellationToken cancellationToken)
    {
        var alignmentSetId = (Guid)parentSnapshot.GetId();

        _mergeContext.Logger.LogInformation("Loading tokenized corpora token locations into cache for creating Alignments");

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Loading token locations into cache",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, CancellationToken cancellationToken) => {

                var alignmentSet = projectDbContext.AlignmentSets
                    .Include(e => e.ParallelCorpus)
                    .Where(e => e.Id == alignmentSetId)
                    .FirstOrDefault();

                if (alignmentSet is null)
                    throw new InvalidModelStateException($"No alignment set found for Id '{alignmentSetId}' when about to create alignment children");

                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, alignmentSet.ParallelCorpus!.SourceTokenizedCorpusId, cache);
                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, alignmentSet.ParallelCorpus!.TargetTokenizedCorpusId, cache);

                await Task.CompletedTask;
            });

        _mergeContext.Logger.LogInformation("Starting create Alignments");

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
                    count++;
                }
                _mergeContext.MergeBehavior.CompleteInsertModelCommand(firstAlignment.EntityType);
            }
        }

        _mergeContext.Logger.LogInformation($"Completed create Alignments (count: {count})");
    }
}

