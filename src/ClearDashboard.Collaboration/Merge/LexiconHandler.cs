using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class LexiconHandler : DefaultMergeHandler<IModelSnapshot<Models.Lexicon_Lexeme>>
{
    public static (Type EntityType, string EntityId, string ItemName) LexemeCacheKey(string lemmaLanguageType) =>
        (typeof(Models.Lexicon_Lexeme), lemmaLanguageType, nameof(Models.Lexicon_Lexeme.Id));

    public static (Type EntityType, string EntityId, string ItemName) MeaningCacheKey(string textlanguageLexemeId) =>
        (typeof(Models.Lexicon_Meaning), textlanguageLexemeId, nameof(Models.Lexicon_Meaning.Id));

    public static readonly Func<string, string, string?, ProjectDbContext, MergeCache, ILogger, Task<Guid>> ValuesToLexemeId = async (lemma, language, type, projectDbContext, cache, logger) =>
    {
        var cacheKey = $"{lemma}_{language}_{type ?? string.Empty}";

        if (!cache.TryLookupCacheEntry(LexemeCacheKey(cacheKey),
            nameof(Models.Lexicon_Lexeme.Id), out var cacheLexemeId))
        {
            var dbLexemeId = await projectDbContext.Lexicon_Lexemes
                .Where(e => e.Lemma == lemma)
                .Where(e => e.Language == language)
                .Where(e => e.Type == type)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (dbLexemeId != default)
            {
                cache.AddCacheEntry(
                    LexemeCacheKey(cacheKey),
                    nameof(Models.Lexicon_Lexeme.Id), dbLexemeId);

                cacheLexemeId = dbLexemeId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        logger.LogDebug($"Converted Lexeme Lemma ('{lemma}') / Language ('{language}') / Type ('{type}') to LexemeId ('{cacheLexemeId}')");
        return (Guid)cacheLexemeId!;
    };

    public static readonly Func<string, string, Guid, ProjectDbContext, MergeCache, ILogger, Task<Guid>> ValuesToMeaningId = async (text, language, lexemeId, projectDbContext, cache, logger) =>
    {
        var cacheKey = $"{text}_{language}_{lexemeId}";

        if (!cache.TryLookupCacheEntry(MeaningCacheKey(cacheKey),
            nameof(Models.Lexicon_Meaning.Id), out var cacheMeaningId))
        {
            var dbMeaningId = await projectDbContext.Lexicon_Meanings
                .Where(e => e.LexemeId == lexemeId)
                .Where(e => e.Text == text)
                .Where(e => e.Language == language)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            if (dbMeaningId != default)
            {
                cache.AddCacheEntry(
                    MeaningCacheKey(cacheKey),
                    nameof(Models.Lexicon_Meaning.Id), dbMeaningId);

                cacheMeaningId = dbMeaningId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        logger.LogDebug($"Converted Meaning Text ('{text}') / Language ('{language}') / LexemeId ('{lexemeId}') to MeaningId ('{cacheMeaningId}')");
        return (Guid)cacheMeaningId!;
    };

    public static readonly Func<string, Guid, ProjectDbContext, ILogger, Task<Guid>> ValuesToFormId = async (text, lexemeId, projectDbContext, logger) =>
    {
        var formId = await projectDbContext.Lexicon_Forms
            .Where(e => e.LexemeId == lexemeId)
            .Where(e => e.Text == text)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Form Text ('{text}') / LexemeId ('{lexemeId}') to FormId ('{formId}')");
        return formId;
    };

    public static readonly Func<string, Guid, ProjectDbContext, ILogger, Task<Guid>> ValuesToTranslationId = async (text, meaningId, projectDbContext, logger) =>
    {
        var translationId = await projectDbContext.Lexicon_Translations
            .Where(e => e.MeaningId == meaningId)
            .Where(e => e.Text == text)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Translation Text ('{text}') / MeaningId ('{meaningId}') to TranslationId ('{translationId}')");
        return translationId;
    };

    public LexiconHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Lexeme), nameof(Models.Lexicon_Lexeme.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var (lemma, language, type) = ExtractLexemeIdParts(modelSnapshot);
                var lexemeId = await ValuesToLexemeId(lemma, language, type, projectDbContext, cache, logger);
                return (lexemeId != default) ? lexemeId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Meaning), nameof(Models.Lexicon_Meaning.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {
                
                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var (text, language) = ExtractMeaningIdParts(modelSnapshot);
                var meaningId = await ValuesToMeaningId(text, language, lexemeId, projectDbContext, cache, logger);
                return (meaningId != default) ? meaningId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Meaning), nameof(Models.Lexicon_Meaning.LexemeId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                return (lexemeId != default) ? lexemeId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Form), nameof(Models.Lexicon_Form.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var text = ExtractStringProperty(modelSnapshot, nameof(Models.Lexicon_Form.Text));
                var formId = await ValuesToFormId(text, lexemeId, projectDbContext, logger);
                return (formId != default) ? formId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Form), nameof(Models.Lexicon_Form.LexemeId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                return (lexemeId != default) ? lexemeId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Translation), nameof(Models.Lexicon_Translation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var meaningId = await MeaningRefLexemeIdToMeaningId(modelSnapshot, lexemeId, projectDbContext, cache, logger);
                if (meaningId == default)
                    return null;

                var text = ExtractStringProperty(modelSnapshot, nameof(Models.Lexicon_Translation.Text));
                var translationId = await ValuesToTranslationId(text, meaningId, projectDbContext, logger);
                return (translationId != default) ? translationId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Translation), nameof(Models.Lexicon_Translation.MeaningId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var meaningId = await MeaningRefLexemeIdToMeaningId(modelSnapshot, lexemeId, projectDbContext, cache, logger);
                return (meaningId != default) ? meaningId : null;
            });

        var L_TYPE = typeof(Models.Lexicon_Lexeme);
        var M_TYPE = typeof(Models.Lexicon_Meaning);
        var F_TYPE = typeof(Models.Lexicon_Form);
        var T_TYPE = typeof(Models.Lexicon_Translation);
        var refName = LexiconBuilder.BuildPropertyRefName();
        var idName = nameof(Models.Lexicon_Lexeme.Id);

        mergeContext.MergeBehavior.AddIdPropertyNameMapping((L_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddIdPropertyNameMapping((M_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddIdPropertyNameMapping((F_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddIdPropertyNameMapping((T_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddPropertyNameMapping(  (L_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddPropertyNameMapping(  (M_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddPropertyNameMapping(  (F_TYPE, refName), new[] { idName });
        mergeContext.MergeBehavior.AddPropertyNameMapping(  (T_TYPE, refName), new[] { idName });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (M_TYPE, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX)),
            new[] { nameof(Models.Lexicon_Meaning.LexemeId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (F_TYPE, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX)),
            new[] { nameof(Models.Lexicon_Form.LexemeId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (T_TYPE, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX)),
            Enumerable.Empty<string>());

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (T_TYPE, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.MEANING_REF_PREFIX)),
            new[] { nameof(Models.Lexicon_Translation.MeaningId) });
    }

    private static (string Lemma, string Language, string? Type) ExtractLexemeIdParts(IModelSnapshot modelSnapshot)
    {
        if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Lexeme.Lemma), out var lemma) &&
            modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Lexeme.Language), out var language))
        {
            modelSnapshot.TryGetNullableStringPropertyValue(nameof(Models.Lexicon_Lexeme.Type), out var type);

            return (lemma, language, type);
        }
        else
        {
            throw new PropertyResolutionException($"{modelSnapshot.EntityType} snapshot does not have both of text, language property values, which are required for Id resolution.");
        }
    }

    private static (string Text, string Language) ExtractMeaningIdParts(IModelSnapshot modelSnapshot)
    {
        if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Text), out var text) &&
            modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Language), out var language))
        {
            return (text, language);
        }
        else
        {
            throw new PropertyResolutionException($"{modelSnapshot.EntityType} snapshot does not have text, language property values, which are required for Id resolution.");
        }
    }

    public static string ExtractStringProperty(IModelSnapshot modelSnapshot, string propertyName)
    {
        if (modelSnapshot.TryGetStringPropertyValue(propertyName, out var refValue))
        {
            return refValue;
        }
        else
        {
            throw new PropertyResolutionException($"{modelSnapshot.EntityType} snapshot does not have {propertyName} property value, which is required for Id resolution.");
        }
    }

    public static async Task<Guid> LexemeRefToLexemeId(IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger)
    {
        var lexemeRef = ExtractStringProperty(modelSnapshot, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX));

        var (lexemeLemma, lexemeLanguage, lexemeType) = LexiconBuilder.DecodeLexemeRef(lexemeRef);
        var lexemeId = await ValuesToLexemeId(lexemeLemma, lexemeLanguage, lexemeType, projectDbContext, cache, logger);

        return lexemeId;
    }

    public static async Task<Guid> MeaningRefLexemeIdToMeaningId(IModelSnapshot modelSnapshot, Guid lexemeId, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger)
    {
        var meaningRef = ExtractStringProperty(modelSnapshot, LexiconBuilder.BuildPropertyRefName(LexiconBuilder.MEANING_REF_PREFIX));

        var (meaningText, meaningLanguage) = LexiconBuilder.DecodeMeaningRef(meaningRef);
        var meaningId = await ValuesToMeaningId(meaningText, meaningLanguage, lexemeId, projectDbContext, cache, logger);

        return meaningId;
    }

}