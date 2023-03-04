﻿using System;
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
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public class TranslationHandler : DefaultMergeHandler
{
    public static Guid LookupSourceTokenizedCorpusId(ProjectDbContext projectDbContext, Guid translationSetId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry((typeof(Models.TranslationSet), translationSetId.ToString()!),
            nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId))
        {
            sourceTokenizedCorpusId = projectDbContext.TranslationSets
                .Include(e => e.ParallelCorpus)
                .Where(e => e.Id == translationSetId!)
                .Select(e => e.ParallelCorpus!.SourceTokenizedCorpusId)
                .FirstOrDefault();

            if (sourceTokenizedCorpusId is null) throw new InvalidModelStateException($"Invalid TranslationSetId '{translationSetId}' - SourceTokenizedCorpusId not found");

            cache.AddCacheEntry(
                (typeof(Models.TranslationSet), translationSetId!.ToString()!),
                nameof(Models.Translation.SourceTokenComponentId), sourceTokenizedCorpusId);
        }

        return (Guid)sourceTokenizedCorpusId!;
    }

    public TranslationHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Translation), nameof(Models.Translation.SourceTokenComponentId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

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
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Translation>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Translation>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("TranslationSetId", out var translationSetId) &&
                    modelSnapshot.PropertyValues.TryGetValue("SourceTokenLocation", out var SourceTokenLocation))
                {
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)translationSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, cache);

                    var translationId = projectDbContext.Translations
                        .Where(e => e.TranslationSetId == (Guid)translationSetId!)
                        .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                        .Select(e => e.Id)
                        .FirstOrDefault();

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
    }
}

