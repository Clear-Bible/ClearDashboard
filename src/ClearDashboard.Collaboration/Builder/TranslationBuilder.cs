using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Builder;

public class TranslationBuilder : GeneralModelBuilder<Models.Translation>
{
    public const string SOURCE_TOKEN_LOCATION = "SourceTokenLocation";
    public const string BOOK_CHAPTER_LOCATION = "Location";

    public override string IdentityKey => BuildPropertyRefName();
    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { SOURCE_TOKEN_LOCATION, typeof(string) },
            { BOOK_CHAPTER_LOCATION, typeof(string) }
        };

    public static GeneralListModel<GeneralModel<Models.Translation>> BuildModelSnapshots(Guid translationSetId, BuilderContext builderContext)
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

    public static IEnumerable<(Models.Translation translation, Models.Token leadingToken)> GetTranslations(ProjectDbContext projectDbContext, Guid translationSetId)
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
    }
}
