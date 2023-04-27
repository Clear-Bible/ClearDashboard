using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class CorpusBuilder : GeneralModelBuilder<Models.Corpus>
{
    public readonly static Dictionary<Models.CorpusType, Guid> CorpusManuscriptIds = new() {
        { Models.CorpusType.ManuscriptHebrew, Guid.Parse("3D275D10-5374-4649-8D0D-9E69281E5B81") },
        { Models.CorpusType.ManuscriptGreek, Guid.Parse("3D275D10-5374-4649-8D0D-9E69281E5B82") }
    };

    public Func<ProjectDbContext, IEnumerable<Models.Corpus>> GetCorpora = (projectDbContext) =>
    {
        return projectDbContext.Corpa.AsNoTrackingWithIdentityResolution().OrderBy(c => c.Created).ToList();
    };

    public override IEnumerable<GeneralModel<Models.Corpus>> BuildModelSnapshots(BuilderContext builderContext)
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
        if (dbModel.CorpusType == Models.CorpusType.ManuscriptHebrew ||
            dbModel.CorpusType == Models.CorpusType.ManuscriptGreek)
        {
            dbModel.Id = CorpusManuscriptIds[dbModel.CorpusType];
        }

        var modelSnapshot = ExtractUsingModelIds(dbModel, builderContext.CommonIgnoreProperties);
        return modelSnapshot;
    }
}
