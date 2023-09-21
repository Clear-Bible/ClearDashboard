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
    public const string SOURCE_TOKEN_SURFACE_TEXT = "SourceTokenSurfaceText";
    public const string BOOK_CHAPTER_LOCATION = "Location";

    public override string IdentityKey => BuildPropertyRefName();
    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { SOURCE_TOKEN_LOCATION, typeof(string) },
            { SOURCE_TOKEN_SURFACE_TEXT, typeof(string) },
            { BOOK_CHAPTER_LOCATION, typeof(string) }
        };

    // We serialize this in groups where the AlignmentSetId is in the heading, so don't include it here:
    public override IEnumerable<string> NoSerializePropertyNames => new[] { nameof(Models.Translation.TranslationSetId) };

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.Translation translation, Models.Token leadingToken)>> GetTranslations = (projectDbContext, translationSetId) =>
    {
        var translations = projectDbContext.Translations
            .Include(e => e.SourceTokenComponent!)
                .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
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

        // FIXME:  enhance GeneralModelBuilder to use propertyConverter delegates
        // so that it produce "SourceTokenLocation" itself
        // (it has no way of knowing that TranslationSet already specifies (via
        // parallel corpus) which is the real SourceTokenizedCorpusId).  
        var sourceTokenComponentRef = modelProperties["SourceTokenComponentRef"];

        modelProperties.Remove("SourceTokenComponentRef");

        modelProperties.Add(SOURCE_TOKEN_LOCATION,
            (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenLocation));
        modelProperties.Add(SOURCE_TOKEN_SURFACE_TEXT,
            (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenSurfaceText));
        modelProperties.Add(BOOK_CHAPTER_LOCATION,
            (typeof(string), $"{translation.leadingToken.BookNumber:000}{translation.leadingToken.ChapterNumber:000}"));

        var identityPropertyValue = (
            translation.translation.TranslationSetId.ToString() +
            modelProperties[SOURCE_TOKEN_LOCATION]
        ).ToMD5String();

        var translationModelSnapshot = new GeneralModel<Models.Translation>(BuildPropertyRefName(), $"Translation_{identityPropertyValue}");
        GeneralModelBuilder<Models.Translation>.AddPropertyValuesToGeneralModel(translationModelSnapshot, modelProperties);

        return translationModelSnapshot;
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
