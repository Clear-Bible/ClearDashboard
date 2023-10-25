using Caliburn.Micro;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System.Diagnostics;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public class TranslationSetHandler : DefaultMergeHandler<IModelSnapshot<Models.TranslationSet>>
{
    public TranslationSetHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    public static (Type EntityType, string EntityId, string ItemName) TranslationSetCacheKey(Guid translationSetId) =>
        (typeof(Models.TranslationSet), translationSetId.ToString()!, nameof(Models.TranslationSet));

    protected override async Task HandleCreateChildrenAsync(IModelSnapshot<Models.TranslationSet> parentSnapshot, CancellationToken cancellationToken)
    {
        var translationSetId = (Guid)parentSnapshot.GetId();

        _mergeContext.Logger.LogInformation("Loading tokenized corpora token locations into cache for creating Translations");

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Loading token locations into cache",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var translationSets = projectDbContext.TranslationSets
                    .Include(e => e.ParallelCorpus)
                    .Where(e => e.Id == translationSetId)
                    .FirstOrDefault();

                if (translationSets is null)
                    throw new InvalidModelStateException($"No translation set found for Id '{translationSetId}' when about to create translation children");

                LoadTokenizedCorpusLocationsIntoCache(projectDbContext, translationSets.ParallelCorpus!.SourceTokenizedCorpusId, cache);

                await Task.CompletedTask;
            },
            cancellationToken);

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


                try
                {
                    foreach (var child in parentSnapshot.Children[translationsChildName])
                    {
                        var id = await _mergeContext.MergeBehavior.RunInsertModelCommand((IModelSnapshot)child, cancellationToken);
                        count++;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }


                try
                {
                    _mergeContext.MergeBehavior.CompleteInsertModelCommand(firstTranslation.EntityType);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

            }
        }

        _mergeContext.Logger.LogInformation($"Completed create Translations (count: {count})");
    }
}

