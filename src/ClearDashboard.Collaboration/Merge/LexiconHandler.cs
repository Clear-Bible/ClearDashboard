using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using Paratext.PluginInterfaces;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Translation;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using ClearDashboard.DataAccessLayer.Models;

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

                if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Lexeme.Lemma), out var lemma) &&
                    modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Lexeme.Language), out var language))
                {
                    modelSnapshot.TryGetNullableStringPropertyValue(nameof(Models.Lexicon_Lexeme.Type), out var type);

                    var lexemeId = await ValuesToLexemeId(lemma, language, type, projectDbContext, cache, logger);
                    return (lexemeId != default) ? lexemeId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Meaning snapshot does not have both of text, language property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Meaning), nameof(Models.Lexicon_Meaning.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Text), out var text) &&
                    modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Meaning.Language), out var language) &&
                    modelSnapshot.TryGetStringPropertyValue(LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX), out var lexemeRef))
                {
                    var (lexemeLemma, lexemeLanguage, lexemeType) = LexiconBuilder.DecodeLexemeRef(lexemeRef);
                    var lexemeId = await ValuesToLexemeId(lexemeLemma, lexemeLanguage, lexemeType, projectDbContext, cache, logger);

                    if (lexemeId == default)
                        return null;

                    var meaningId = await ValuesToMeaningId(text, language, lexemeId, projectDbContext, logger);
                    return (meaningId != default) ? meaningId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Meaning snapshot does not have all of text, language, LexemeRef property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Form), nameof(Models.Lexicon_Form.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Form.Text), out var text) &&
                    modelSnapshot.TryGetStringPropertyValue(LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX), out var lexemeRef))
                {
                    var (lexemeLemma, lexemeLanguage, lexemeType) = LexiconBuilder.DecodeLexemeRef(lexemeRef);
                    var lexemeId = await ValuesToLexemeId(lexemeLemma, lexemeLanguage, lexemeType, projectDbContext, logger);

                    if (lexemeId == default)
                        return null;

                    var formId = await ValuesToFormId(text, lexemeId, projectDbContext, logger);
                    return (formId != default) ? formId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Form snapshot does not have all of text, LexemeRef property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_Translation), nameof(Models.Lexicon_Translation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Lexicon_Translation.Text), out var text) &&
                    modelSnapshot.TryGetStringPropertyValue(LexiconBuilder.BuildPropertyRefName(LexiconBuilder.MEANING_REF_PREFIX), out var meaningRef) &&
                    modelSnapshot.TryGetStringPropertyValue(LexiconBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX), out var lexemeRef))
                {
                    var (lexemeLemma, lexemeLanguage, lexemeType) = LexiconBuilder.DecodeLexemeRef(lexemeRef);
                    var lexemeId = await ValuesToLexemeId(lexemeLemma, lexemeLanguage, lexemeType, projectDbContext, logger);

                    if (lexemeId == default)
                        return null;

                    var (meaningText, meaningLanguage) = LexiconBuilder.DecodeMeaningRef(meaningRef);
                    var meaningId = await ValuesToMeaningId(meaningText, meaningLanguage, lexemeId, projectDbContext, logger);

                    if (meaningId == default)
                        return null;

                    var formId = await ValuesToTranslationId(text, meaningId, projectDbContext, logger);
                    return (formId != default) ? formId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Form snapshot does not have all of text, LexemeRef property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_Lexeme), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Lexeme.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_Lexeme), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Lexeme.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_Meaning), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Meaning.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_Meaning), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Meaning.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_Form), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Form.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_Form), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Form.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_Translation), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Translation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_Translation), LexiconBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_Translation.Id) });
    }

}