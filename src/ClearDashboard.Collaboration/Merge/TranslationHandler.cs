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

                if (modelSnapshot.PropertyValues.TryGetValue(TranslationBuilder.SOURCE_TOKEN_LOCATION, out var SourceTokenLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Translation.TranslationSetId), out var translationSetId))
                {
                    var sourceTokenDeleted = false;
                    if (modelSnapshot.TryGetPropertyValue(AlignmentBuilder.SOURCE_TOKEN_DELETED, out var std))
                    {
                        sourceTokenDeleted = (bool)std!;
                    }
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)translationSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, sourceTokenDeleted, true, cache);

                    logger.LogDebug($"Converted Translation having SourceTokenLocation ('{SourceTokenLocation}') / SourceTokenDeleted ({sourceTokenDeleted}) / TranslationSetId ('{translationSetId}') to SourceTokenComponentId ('{sourceTokenComponentId}')");
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

                if (modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Translation.TranslationSetId), out var translationSetId) &&
                    modelSnapshot.PropertyValues.TryGetValue(TranslationBuilder.SOURCE_TOKEN_LOCATION, out var SourceTokenLocation))
                {
                    var sourceTokenDeleted = false;
                    if (modelSnapshot.TryGetPropertyValue(AlignmentBuilder.SOURCE_TOKEN_DELETED, out var std))
                    {
                        sourceTokenDeleted = (bool)std!;
                    }
                    var sourceTokenizedCorpusId = LookupSourceTokenizedCorpusId(projectDbContext, (Guid)translationSetId!, cache);
                    var sourceTokenComponentId = LookupTokenComponent(projectDbContext, sourceTokenizedCorpusId, (string)SourceTokenLocation!, sourceTokenDeleted, false, cache);

                    var translationId = await projectDbContext.Translations
                        .Where(e => e.TranslationSetId == (Guid)translationSetId!)
                        .Where(e => e.SourceTokenComponentId == sourceTokenComponentId)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    if (translationId != default)
                    {
                        logger.LogDebug($"Resolved TranslationSetId ('{translationSetId}') / SourceTokenComponentId ('{sourceTokenComponentId}') / SourceTokenDeleted ({sourceTokenDeleted}) to Id ('{translationId}')");
                        return translationId;
                    }

                    return null;
                }
                else
                {
                    throw new PropertyResolutionException($"Translation snapshot does not have both:  TranslationSetId+SourceTokenComponentRef+TargetTokenComponentRef, which are required for Id resolution.");
                }
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Translation), nameof(Models.Translation.LexiconTranslationId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Translation>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Translation>");
                }

                var refName = TranslationBuilder.BuildPropertyRefName(TranslationBuilder.LEXICONTRANSLATION_REF_PREFIX);
                if (modelSnapshot.TryGetStringPropertyValue(refName, out var lexiconTranslationRef))
                {
                    var (lexemeLemma, lexemeLanguage, lexemeType, meaningText, meaningLanguage, translationText) =
                        TranslationBuilder.DecodeLexiconTranslationRef(lexiconTranslationRef);

                    var lexemeId = await LexiconHandler.ValuesToLexemeId(lexemeLemma, lexemeLanguage, lexemeType, projectDbContext, cache, logger);
                    if (lexemeId == default)
                        throw new PropertyResolutionException($"{lexiconTranslationRef} cannot be resolved to a Lexicon_Lexeme");

                    var meaningId = await LexiconHandler.ValuesToMeaningId(meaningText, meaningLanguage, lexemeId, projectDbContext, cache, logger);
                    if (meaningId == default)
                        throw new PropertyResolutionException($"{lexiconTranslationRef} cannot be resolved to a Lexicon_Meaning");

                    var translationId = await LexiconHandler.ValuesToTranslationId(translationText, meaningId, projectDbContext, logger);
                    if (translationId == default)
                        throw new PropertyResolutionException($"{lexiconTranslationRef} cannot be resolved to a Lexicon_Translation");

                    logger.LogDebug($"Resolved {lexiconTranslationRef} to Id ('{translationId}')");
                    return translationId;
                }
                else
                {
                    return null;
                }
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.REF),
            new[] { nameof(Models.Translation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.REF),
            new[] { nameof(Models.Translation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.SOURCE_TOKEN_LOCATION),
            new[] { nameof(Models.Translation.SourceTokenComponentId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.BuildPropertyRefName(TranslationBuilder.LEXICONTRANSLATION_REF_PREFIX)),
            new[] { nameof(Models.Translation.LexiconTranslationId) });

        // MergeBehaviorBase.CreateModelSnapshotUpdateCommand,
        // MergeBehaviorBase.ApplyPropertyValueDifferencesToCommand and
        // MergeBehaviorBase.ApplyPropertyModelDifferencesToCommand are now
        // coded to allow multiple property names to be mapped to a single
        // entity property name (see MergeBehaviorBase._propertyNameMap) and
        // only update that entity property value a single time.  These 
        // property name mappings are only used when modifying an entity.
        // So per the two new 'add' statements below, now changes to either
        // SOURCE_TOKEN_LOCATION (above) or
        // SOURCE_TOKEN_DELETED (below) will trigger
        // an update to SourceTokenComponentId.  

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Translation), TranslationBuilder.SOURCE_TOKEN_DELETED),
            new[] { nameof(Models.Translation.SourceTokenComponentId) });
    }
}

