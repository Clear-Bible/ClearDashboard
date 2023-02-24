using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class TranslationSetBuilder : GeneralModelBuilder<Models.TranslationSet>
{
    public static IEnumerable<GeneralModel<Models.TranslationSet>> BuildModelSnapshot(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.TranslationSet>>();

        var modelItems = GetTranslationSets(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public static GeneralModel<Models.TranslationSet> BuildModelSnapshot(Models.TranslationSet translationSet, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(translationSet, builderContext.CommonIgnoreProperties);

        if (translationSet.Translations.Any())
        {
            var translationModelSnapshots = new GeneralListModel<GeneralModel<Models.Translation>>();
            foreach (var translation in translationSet.Translations)
            {
                var modelProperties = GeneralModelBuilder<Models.Translation>.ExtractUsingModelRefs(translation, builderContext, new List<string>() { "Id" });

                // FIXME:  enhance GeneralModelBuilder to use propertyConverter delegates
                // so that it produce "SourceTokenLocation" itself
                // (it has no way of knowing that TranslationSet already specifies (via
                // parallel corpus) which is the real SourceTokenizedCorpusId).  
                var sourceTokenComponentRef = modelProperties["SourceTokenComponentRef"];

                modelProperties.Remove("SourceTokenComponentRef");

                modelProperties.Add("SourceTokenComponentLocation",
                    (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenLocation));

                var identityPropertyValue = (
                    translation.TranslationSetId.ToString() +
                    modelProperties["SourceTokenComponentLocation"]
                ).ToMD5String();

                var translationModelSnapshot = new GeneralModel<Models.Translation>(BuildPropertyRefName(), $"Translation_{identityPropertyValue}");
                GeneralModelBuilder<Models.Translation>.AddPropertyValuesToGenericModel(translationModelSnapshot, modelProperties);

                translationModelSnapshots.Add(translationModelSnapshot);
            }
            modelSnapshot.AddChild("Translations", translationModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static IEnumerable<Models.TranslationSet> GetTranslationSets(ProjectDbContext projectDbContext)
    {
        return projectDbContext.TranslationSets
            .Include(ts => ts.Translations.OrderBy(t => t.Created)).ThenInclude(a => a.SourceTokenComponent)
            .OrderBy(c => c.Created)
            .ToList();
    }

    public static TranslationRef BuildTranslationRef(
        Models.Translation translation,
        BuilderContext builderContext)
    {
        return new TranslationRef
        {
            TranslationSetId = translation.TranslationSetId,
            SourceTokenRef = TokenBuilder.BuildTokenRef(translation.SourceTokenComponent!, builderContext),
        };
    }
}
