using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class TranslationSetBuilder : GeneralModelBuilder<Models.TranslationSet>
{
    public override IEnumerable<GeneralModel<Models.TranslationSet>> BuildModelSnapshots(BuilderContext builderContext)
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
        var sourceTokenizedCorpusId = translationSet.ParallelCorpus!.SourceTokenizedCorpus!.Id;

        modelSnapshot.AddChild("Translations", TranslationBuilder.BuildModelSnapshots(translationSet.Id, builderContext).AsModelSnapshotChildrenList());

        return modelSnapshot;
    }

    public static IEnumerable<Models.TranslationSet> GetTranslationSets(ProjectDbContext projectDbContext)
    {
        return projectDbContext.TranslationSets
            .Include(e => e.ParallelCorpus)
                .ThenInclude(e => e!.SourceTokenizedCorpus)
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
