using System;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Collaboration.Merge;

public class TranslationSetHandler : DefaultMergeHandler
{
	public TranslationSetHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    protected override async Task CreateChildrenAsync<T>(T parentSnapshot, CancellationToken cancellationToken)
    {
        var translationSetId = (Guid)parentSnapshot.GetId();

        _mergeContext.Logger.LogInformation("Loading tokenized corpora token locations into cache for creating Translations");

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Loading token locations into cache",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, CancellationToken cancellationToken) => {

                var translationSets = projectDbContext.TranslationSets
                    .Include(e => e.ParallelCorpus)
                    .Where(e => e.Id == translationSetId)
                    .FirstOrDefault();

                if (translationSets is null)
                    throw new InvalidModelStateException($"No translation set found for Id '{translationSetId}' when about to create translation children");

                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, translationSets.ParallelCorpus!.SourceTokenizedCorpusId, cache);

                await Task.CompletedTask;
            });

        _mergeContext.Logger.LogInformation("Starting create Translations");

        var count = 0;
        var translationsChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.Translation)].childName;
        if (parentSnapshot.Children.ContainsKey(translationsChildName))
        {
            var firstChild = parentSnapshot.Children[translationsChildName].FirstOrDefault();
            if (firstChild is not null)
            {
                var firstTranslation = (IModelSnapshot)firstChild;
                var handler = _mergeContext.FindMergeHandler<IModelSnapshot<Models.Translation>>();

                _mergeContext.MergeBehavior.StartInsertModelCommand(firstTranslation);
                foreach (var child in parentSnapshot.Children[translationsChildName])
                {
                    var id = await _mergeContext.MergeBehavior.RunInsertModelCommand((IModelSnapshot)child, cancellationToken);
                    count++;
                }
                _mergeContext.MergeBehavior.CompleteInsertModelCommand(firstTranslation.EntityType);
            }
        }

        _mergeContext.Logger.LogInformation($"Completed create Translations (count: {count})");
    }
}

