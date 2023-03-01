using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class ParallelCorpusBuilder : GeneralModelBuilder<Models.ParallelCorpus>
{
    public override IEnumerable<GeneralModel<Models.ParallelCorpus>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.ParallelCorpus>>();

        var modelItems = GetParallelCorpora(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public static GeneralModel<Models.ParallelCorpus> BuildModelSnapshot(Models.ParallelCorpus parallelCorpus, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(parallelCorpus, builderContext.CommonIgnoreProperties);
        // TODO:  VerseMappings (once they can be altered in Dashboard)
        // TODO:  Verses (once they can be altered in Dashboard)
        // TODO:  TokenVerseAssociations (once they can be created in Dashboard)

        var tokenCompositeTokens = TokenBuilder.GetParallelCorpusCompositeTokens(builderContext.ProjectDbContext, parallelCorpus.Id);
        if (tokenCompositeTokens.Any())
        {
            var compositeModelSnapshots = new GeneralListModel<GeneralModel<Models.TokenComposite>>();

            foreach (var tct in tokenCompositeTokens)
            {
                compositeModelSnapshots.Add(TokenBuilder.BuildModelSnapshot(tct.TokenComposite, tct.Tokens, builderContext));
            }

            modelSnapshot.AddChild("CompositeTokens", compositeModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static IEnumerable<Models.ParallelCorpus> GetParallelCorpora(ProjectDbContext projectDbContext)
    {
        return projectDbContext.ParallelCorpa.OrderBy(c => c.Created).ToList();
    }
}
