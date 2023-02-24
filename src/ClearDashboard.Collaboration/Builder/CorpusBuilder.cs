using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class CorpusBuilder : GeneralModelBuilder<Models.Corpus>
{
    public static IEnumerable<GeneralModel<Models.Corpus>> BuildModelSnapshot(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Corpus>>();

        var dbModels = GetCorpora(builderContext.ProjectDbContext);
        foreach (var dbModelItem in dbModels)
        {
            modelSnapshots.Add(BuildModelSnapshot(dbModelItem, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.Corpus> BuildModelSnapshot(Models.Corpus dbModel, BuilderContext builderContext)
    {
        //builderContext.IncrementModelNameIndexValue(nameof(Models.Corpus));

        var modelSnapshot = ExtractUsingModelIds(dbModel, builderContext.CommonIgnoreProperties);

        //if (corpus.TokenizedCorpora.Any())
        //{
        //    var tokenizedCorporaExternalData = new List<SnapshotEntityModel<Models.TokenizedCorpus>>();
        //    foreach (var tokenizedCorpus in corpus.TokenizedCorpora)
        //    {
        //        tokenizedCorporaExternalData.Add(TokenizedCorpusBuilder.BuildSnapshotModel(tokenizedCorpus, builderContext));
        //    }

        //    corpusExternalData.AddChild(nameof(Models.Corpus.TokenizedCorpora), tokenizedCorporaExternalData);
        //}

        return modelSnapshot;
    }

    public static IEnumerable<Models.Corpus> GetCorpora(ProjectDbContext projectDbContext)
    {
        return projectDbContext.Corpa.OrderBy(c => c.Created).ToList();
    }
}
