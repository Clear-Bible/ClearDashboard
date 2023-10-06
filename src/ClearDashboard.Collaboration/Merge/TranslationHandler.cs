using System;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using System.Data.Common;

namespace ClearDashboard.Collaboration.Merge;

public class TranslationHandler : DefaultMergeHandler<IModelSnapshot<Models.Translation>>
{
    public static Guid LookupSourceTokenizedCorpusId(ProjectDbContext projectDbContext, Guid translationSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry(TranslationSetHandler.TranslationSetCacheKey(translationSetId),
            nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId))
        {
            sourceTokenizedCorpusId = projectDbContext.TranslationSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == translationSetId!)
                .Select(e => e.ParallelCorpus!.SourceTokenizedCorpusId)
                .FirstOrDefault();

            if (sourceTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid TranslationSetId '{translationSetId}' - SourceTokenizedCorpusId not found");

            cache.AddCacheEntry(
                TranslationSetHandler.TranslationSetCacheKey(translationSetId),
                nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), sourceTokenizedCorpusId);
        }

        return (Guid)sourceTokenizedCorpusId!;
    }

    public TranslationHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Translation), nameof(Models.Translation.SourceTokenComponentId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                await Task.CompletedTask;
                if (modelSnapshot is not IModelSnapshot<Models.Translation>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Translation>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("SourceTokenLocation", out var SourceTokenLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Translation.TranslationSetId), out var translationSetId))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)translationSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, cache);

                    logger.LogDebug($"Converted Translation having SourceTokenLocation ('{SourceTokenLocation}') / TranslationSetId ('{translationSetId}') to SourceTokenComponentId ('{sourceTokenComponentId}')");
                    return sourceTokenComponentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Translation snapshot does not have both TranslationSetId+SourceTokenLocation, which are required for SourceTokenComponentId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Translation), nameof(Models.Translation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Translation>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Translation>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("TranslationSetId", out var translationSetId) &&
                    modelSnapshot.PropertyValues.TryGetValue("SourceTokenLocation", out var SourceTokenLocation))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)translationSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, cache);

                    var translationId = await projectDbContext.Translations
                        .Where(e => e.TranslationSetId == (Guid)translationSetId!)
                        .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    if (translationId == default)
                        throw new PropertyResolutionException($"TranslationSetId '{translationSetId}' and SourceTokenComponentId '{sourceTokenComponentId}' cannot be resolved to a Translation");

                    logger.LogDebug($"Resolved TranslationSetId ('{translationSetId}') / SourceTokenComponentId ('{sourceTokenComponentId}') to Id ('{translationId}')");
                    return translationId;
                }
                else
                {
                    throw new PropertyResolutionException($"Translation snapshot does not have both:  TranslationSetId+SourceTokenComponentRef+TargetTokenComponentRef, which are required for Id resolution.");
                }
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Translation), "Ref"),
            new[] { nameof(Models.Translation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), "Ref"),
            new[] { nameof(Models.Translation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), "SourceTokenLocation"),
            new[] { nameof(Models.Translation.SourceTokenComponentId) });

        // By mapping SourceTokenSurfaceText to an empty property name string, we effectively
        // leave it out of the inserting/updating part of Merge:
        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.SOURCE_TOKEN_SURFACE_TEXT),
            Enumerable.Empty<string>());
    }
}

