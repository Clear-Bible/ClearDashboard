using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Collaboration.Factory;

namespace ClearDashboard.Collaboration.Builder;

public class TranslationBuilder : GeneralModelBuilder<Models.Translation>
{
    public const string SOURCE_TOKEN_LOCATION = "SourceTokenLocation";
    public const string BOOK_CHAPTER_LOCATION = "Location";

    public const string LEXICONTRANSLATION_REF_PREFIX = "LexiconTr";

    public override string IdentityKey => BuildPropertyRefName();
    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { SOURCE_TOKEN_LOCATION, typeof(string) },
            { BOOK_CHAPTER_LOCATION, typeof(string) },
            { BuildPropertyRefName(LEXICONTRANSLATION_REF_PREFIX), typeof(string) }
        };

    // We serialize this in groups where the AlignmentSetId is in the heading, so don't include it here:
    public override IEnumerable<string> NoSerializePropertyNames => new[] { nameof(Models.Translation.TranslationSetId) };

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.Translation translation, Models.Token leadingToken)>> GetTranslations = (projectDbContext, translationSetId) =>
    {
        var translations = projectDbContext.Translations
            .Include(e => e.SourceTokenComponent!)
                .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
            .Include(e => e.LexiconTranslation)
                .ThenInclude(e => e!.Meaning)
                    .ThenInclude(e => e!.Lexeme)
            .Where(e => e.TranslationSetId == translationSetId)
            .Where(e => e.Deleted == null)
            .ToList()
            .Select(e => (
                translation: e,
                leadingToken: e.SourceTokenComponent as Models.Token ??
                             (e.SourceTokenComponent as Models.TokenComposite)!.Tokens.First()
            ))
            .OrderBy(e => e.leadingToken.EngineTokenId);

        // Including 'leadingToken' so that even if a Translation has a composite
        // as its SourceTokenComponent, we can pull book and chapter numbers for
        // grouping during serialization
        return translations;
    };

    public GeneralListModel<GeneralModel<Models.Translation>> BuildModelSnapshots(Guid translationSetId, BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Translation>>();

        var translations = GetTranslations(builderContext.ProjectDbContext, translationSetId);
        foreach (var translation in translations)
        {
            modelSnapshots.Add(BuildModelSnapshot(translation, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.Translation> BuildModelSnapshot((Models.Translation translation, Models.Token leadingToken) translation, BuilderContext builderContext)
    {
        var modelProperties = GeneralModelBuilder<Models.Translation>.ExtractUsingModelRefs(translation.translation, builderContext, new List<string>() { "Id" });

        var lexiconTranslationRefName = BuildPropertyRefName(LEXICONTRANSLATION_REF_PREFIX);
        if (translation.translation.LexiconTranslation is not null)
        {
            var lexiconTranslationRef = CalculateLexiconTranslationRef(
                translation.translation.LexiconTranslation!.Meaning!.Lexeme!.Lemma!,
                translation.translation.LexiconTranslation!.Meaning!.Lexeme!.Language!,
                translation.translation.LexiconTranslation!.Meaning!.Lexeme!.Type,
                translation.translation.LexiconTranslation!.Meaning!.Text!,
                translation.translation.LexiconTranslation!.Meaning!.Language!,
                translation.translation.LexiconTranslation!.Text!);

            modelProperties.Add(lexiconTranslationRefName,
                (typeof(string), lexiconTranslationRef));
        }
        else
        {
            modelProperties.Add(lexiconTranslationRefName,
                (typeof(string), null));
        }

        // FIXME:  enhance GeneralModelBuilder to use propertyConverter delegates
        // so that it produce "SourceTokenLocation" itself
        // (it has no way of knowing that TranslationSet already specifies (via
        // parallel corpus) which is the real SourceTokenizedCorpusId).  
        var sourceTokenComponentRef = modelProperties["SourceTokenComponentRef"];

        modelProperties.Remove("SourceTokenComponentRef");
        modelProperties.Remove(nameof(Models.Translation.LexiconTranslationId));

        modelProperties.Add(SOURCE_TOKEN_LOCATION,
            (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenLocation));

        modelProperties.Add(BOOK_CHAPTER_LOCATION,
            (typeof(string), $"{translation.leadingToken.BookNumber:000}{translation.leadingToken.ChapterNumber:000}"));

        var identityPropertyValue = (
            translation.translation.TranslationSetId.ToString() +
            modelProperties[SOURCE_TOKEN_LOCATION]
        ).ToMD5String();

        var translationModelSnapshot = new GeneralModel<Models.Translation>(BuildPropertyRefName(), $"Translation_{identityPropertyValue}");
        AddPropertyValuesToGeneralModel(translationModelSnapshot, modelProperties);

        return translationModelSnapshot;
    }

    private static string CalculateLexiconTranslationRef(string lexemeLemma, string lexemeLanguage, string? lexemeType, string meaningText, string meaningLanguage, string translationText)
    {
        return EncodePartsToRef(LEXICONTRANSLATION_REF_PREFIX, lexemeLemma, lexemeLanguage, lexemeType, meaningText, meaningLanguage, translationText);
    }

    public static (string lexemeLemma, string lexemeLanguage, string? lexemeType, string meaningText, string meaningLanguage, string translationText) DecodeLexiconTranslationRef(string refValue)
    {
        var parts = DecodeRefToParts(LEXICONTRANSLATION_REF_PREFIX, refValue, 6);
        return (
            lexemeLemma: parts[0], 
            lexemeLanguage: parts[1], 
            lexemeType: string.IsNullOrEmpty(parts[2]) ? null : parts[2], 
            meaningText: parts[3], 
            meaningLanguage: parts[4], 
            translationText: parts[5]
        );
    }

    public static void SaveTranslations(
        GeneralModel translationSet,
        GeneralListModel<GeneralModel<Models.Translation>> translationSnapshots,
        string childPath,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        var translationsByLocation = translationSnapshots
            .GroupBy(e => (string)e[TranslationBuilder.BOOK_CHAPTER_LOCATION]!)
            .ToDictionary(g => g.Key, g => g
                .Select(e => e)
                .OrderBy(e => (string)e[TranslationBuilder.SOURCE_TOKEN_LOCATION]!)
                .ToGeneralListModel<GeneralModel<Models.Translation>>())
            .OrderBy(kvp => kvp.Key);

        foreach (var translationsForLocation in translationsByLocation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Instead of the more general GeneralModelJsonConverter, this will use the
            // more specific TranslationGroupJsonConverter:
            var translationGroup = new TranslationGroup()
            {
                TranslationSetId = (Guid)translationSet.GetId(),
                SourceTokenizedCorpus = (TokenizedCorpusExtra)translationSet[TranslationSetBuilder.SOURCE_TOKENIZED_CORPUS]!,
                Location = translationsForLocation.Key,
                Items = translationsForLocation.Value
            };
            var serializedChildModelSnapshot = JsonSerializer.Serialize<TranslationGroup>(
                translationGroup,
                options);
            File.WriteAllText(
                Path.Combine(
                    childPath,
                    string.Format(ProjectSnapshotFactoryCommon.TranslationsByLocationFileNameTemplate,
                        translationSet.GetId(),
                        translationsForLocation.Key)),
                serializedChildModelSnapshot);
        }
    }
}
