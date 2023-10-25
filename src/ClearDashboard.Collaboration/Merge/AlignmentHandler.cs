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

public class AlignmentHandler : DefaultMergeHandler<IModelSnapshot<Models.Alignment>>
{
    public static Guid LookupSourceTokenizedCorpusId(ProjectDbContext projectDbContext, Guid alignmentSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry(AlignmentSetHandler.AlignmentSetCacheKey(alignmentSetId),
            nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId))
        {
            sourceTokenizedCorpusId = projectDbContext.AlignmentSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == alignmentSetId!)
                .Select(e => e.ParallelCorpus!.SourceTokenizedCorpusId)
                .FirstOrDefault();

            if (sourceTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid AlignmentSetId '{alignmentSetId}' - SourceTokenizedCorpusId not found");

            cache.AddCacheEntry(
                AlignmentSetHandler.AlignmentSetCacheKey(alignmentSetId),
                nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), sourceTokenizedCorpusId);
        }

        return (Guid)sourceTokenizedCorpusId!;
    }

    public static Guid LookupTargetTokenizedCorpusId(ProjectDbContext projectDbContext, Guid alignmentSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry(AlignmentSetHandler.AlignmentSetCacheKey(alignmentSetId),
            nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), out var targetTokenizedCorpusId))
        {
            targetTokenizedCorpusId = projectDbContext.AlignmentSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == alignmentSetId!)
                .Select(e => e.ParallelCorpus!.TargetTokenizedCorpusId)
                .FirstOrDefault();

            if (targetTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid AlignmentSetId '{alignmentSetId}' - TargetTokenizedCorpusId not found");

            cache.AddCacheEntry(
                AlignmentSetHandler.AlignmentSetCacheKey(alignmentSetId),
                nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), targetTokenizedCorpusId);
        }

        return (Guid)targetTokenizedCorpusId!;
    }

    public AlignmentHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.SourceTokenComponentId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("SourceTokenLocation", out var SourceTokenLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Alignment.AlignmentSetId), out var alignmentSetId))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, cache);

                    logger.LogDebug($"Converted Alignment having SourceTokenLocation ('{SourceTokenLocation}') / AlignmentSetId ('{alignmentSetId}') to SourceTokenComponentId ('{sourceTokenComponentId}')");

                    await Task.CompletedTask;
                    return sourceTokenComponentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Alignment snapshot does not have both AlignmentSetId+SourceTokenLocation, which are required for SourceTokenComponentId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.TargetTokenComponentId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("TargetTokenLocation", out var TargetTokenLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Alignment.AlignmentSetId), out var alignmentSetId))
                {
                    var targetTokenizedCorpusId = LookupTargetTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var targetTokenComponentId = LookupTokenComponent(projectDbContext, targetTokenizedCorpusId, (string)TargetTokenLocation!, cache);

                    logger.LogDebug($"Converted Alignment having TargetTokenLocation ('{TargetTokenLocation}') / AlignmentSetId ('{alignmentSetId}') to TargetTokenComponentId ('{targetTokenComponentId}')");

                    await Task.CompletedTask;
                    return targetTokenComponentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Alignment snapshot does not have both AlignmentSetId+TargetTokenLocation, which are required for TargetTokenComponentId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("AlignmentSetId", out var alignmentSetId) &&
                    modelSnapshot.PropertyValues.TryGetValue("SourceTokenLocation", out var SourceTokenLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue("TargetTokenLocation", out var TargetTokenLocation))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, cache);
                    var targetTokenizedCorpusId = LookupTargetTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var targetTokenComponentId = LookupTokenComponent(projectDbContext, targetTokenizedCorpusId, (string)TargetTokenLocation!, cache);

                    var alignmentId = await projectDbContext.Alignments
                        .Where(e => e.AlignmentSetId == (Guid)alignmentSetId!)
                        .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                        .Where(e => e.TargetTokenComponentId == targetTokenComponentId)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    if (alignmentId == default)
                        throw new PropertyResolutionException($"AlignmentSetId '{alignmentSetId}' and SourceTokenComponentId '{sourceTokenComponentId}' cannot be resolved to a Alignment");

                    logger.LogDebug($"Resolved AlignmentSetId ('{alignmentSetId}') / SourceTokenComponentId ('{sourceTokenComponentId}') to Id ('{alignmentId}')");
                    return alignmentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Alignment snapshot does not have both:  AlignmentSetId+SourceTokenComponentRef+TargetTokenComponentRef, which are required for Id resolution.");
                }
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Alignment), "Ref"),
            new[] { nameof(Models.Alignment.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), "Ref"),
            new[] { nameof(Models.Alignment.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), AlignmentBuilder.SOURCE_TOKEN_LOCATION),
            new[] { nameof(Models.Alignment.SourceTokenComponentId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), AlignmentBuilder.TARGET_TOKEN_LOCATION),
            new[] { nameof(Models.Alignment.TargetTokenComponentId) });

        // By mapping Location to an empty property name string, we effectively
        // leave it out of the inserting/updating part of Merge:
        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), AlignmentBuilder.BOOK_CHAPTER_LOCATION),
            Enumerable.Empty<string>());

        // By mapping SourceTokenSurfaceText to an empty property name string, we effectively
        // leave it out of the inserting/updating part of Merge:
        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), AlignmentBuilder.SOURCE_TOKEN_SURFACE_TEXT),
            Enumerable.Empty<string>());

        // By mapping TargetTokenSurfaceText to an empty property name string, we effectively
        // leave it out of the inserting/updating part of Merge:
        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), AlignmentBuilder.TARGET_TOKEN_SURFACE_TEXT),
            Enumerable.Empty<string>());
    }
}

