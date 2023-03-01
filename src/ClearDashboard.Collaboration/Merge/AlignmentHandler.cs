using System;
using System.Linq.Expressions;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Collaboration.Exceptions;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public class AlignmentHandler : DefaultMergeHandler
{
    public static Guid LookupSourceTokenizedCorpusId(ProjectDbContext projectDbContext, Guid alignmentSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry((typeof(Models.AlignmentSet), alignmentSetId.ToString()!),
            nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId))
        {
            sourceTokenizedCorpusId = projectDbContext.AlignmentSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == alignmentSetId!)
                .Select(e => e.ParallelCorpus!.SourceTokenizedCorpusId)
                .FirstOrDefault();

            if (sourceTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid AlignmentSetId '{alignmentSetId}' - SourceTokenizedCorpusId not found");

            cache.AddCacheEntry(
                (typeof(Models.AlignmentSet), alignmentSetId!.ToString()!),
                nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), sourceTokenizedCorpusId);
        }

        return (Guid)sourceTokenizedCorpusId!;
    }

    public static Guid LookupTargetTokenizedCorpusId(ProjectDbContext projectDbContext, Guid alignmentSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry((typeof(Models.AlignmentSet), alignmentSetId.ToString()!),
            nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), out var targetTokenizedCorpusId))
        {
            targetTokenizedCorpusId = projectDbContext.AlignmentSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == alignmentSetId!)
                .Select(e => e.ParallelCorpus!.TargetTokenizedCorpusId)
                .FirstOrDefault();

            if (targetTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid AlignmentSetId '{alignmentSetId}' - TargetTokenizedCorpusId not found");

            cache.AddCacheEntry(
                (typeof(Models.AlignmentSet), alignmentSetId!.ToString()!),
                nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), targetTokenizedCorpusId);
        }

        return (Guid)targetTokenizedCorpusId!;
    }

    public AlignmentHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.SourceTokenComponentId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.EntityPropertyValues.TryGetValue("SourceTokenComponentLocation", out var sourceTokenComponentLocation) &&
                    modelSnapshot.EntityPropertyValues.TryGetValue(nameof(Models.Alignment.AlignmentSetId), out var alignmentSetId))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)sourceTokenComponentLocation!, cache);

                    logger.LogInformation($"Converted Alignment having SourceTokenComponentLocation ('{sourceTokenComponentLocation}') / AlignmentSetId ('{alignmentSetId}') to SourceTokenComponentId ('{sourceTokenComponentId}')");
                    return sourceTokenComponentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Alignment snapshot does not have both AlignmentSetId+SourceTokenComponentLocation, which are required for SourceTokenComponentId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.TargetTokenComponentId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.EntityPropertyValues.TryGetValue("TargetTokenComponentLocation", out var targetTokenComponentLocation) &&
                    modelSnapshot.EntityPropertyValues.TryGetValue(nameof(Models.Alignment.AlignmentSetId), out var alignmentSetId))
                {
                    var targetTokenizedCorpusId = LookupTargetTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var targetTokenComponentId = LookupTokenComponent(projectDbContext, targetTokenizedCorpusId, (string)targetTokenComponentLocation!, cache);

                    logger.LogInformation($"Converted Alignment having TargetTokenComponentLocation ('{targetTokenComponentLocation}') / AlignmentSetId ('{alignmentSetId}') to TargetTokenComponentId ('{targetTokenComponentId}')");
                    return targetTokenComponentId;
                }
                else
                {
                    throw new PropertyResolutionException($"Alignment snapshot does not have both AlignmentSetId+TargetTokenComponentLocation, which are required for TargetTokenComponentId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Alignment), nameof(Models.Alignment.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Alignment>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Alignment>");
                }

                if (modelSnapshot.EntityPropertyValues.TryGetValue("AlignmentSetId", out var alignmentSetId) &&
                    modelSnapshot.EntityPropertyValues.TryGetValue("SourceTokenComponentLocation", out var sourceTokenComponentLocation))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)alignmentSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)sourceTokenComponentLocation!, cache);

                    var alignmentId = projectDbContext.Alignments
                        .Where(e => e.AlignmentSetId == (Guid)alignmentSetId!)
                        .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    if (alignmentId == default)
                        throw new PropertyResolutionException($"AlignmentSetId '{alignmentSetId}' and SourceTokenComponentId '{sourceTokenComponentId}' cannot be resolved to a Alignment");

                    logger.LogInformation($"Resolved AlignmentSetId ('{alignmentSetId}') / SourceTokenComponentId ('{sourceTokenComponentId}') to Id ('{alignmentId}')");
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
            (typeof(Models.Alignment), "SourceTokenComponentLocation"),
            new[] { nameof(Models.Alignment.SourceTokenComponentId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), "TargetTokenComponentLocation"),
            new[] { nameof(Models.Alignment.TargetTokenComponentId) });

        // By mapping Location to an empty property name string, we effectively
        // leave it out of the inserting/updating part of Merge:
        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Alignment), "Location"),
            Enumerable.Empty<string>());
    }
}

