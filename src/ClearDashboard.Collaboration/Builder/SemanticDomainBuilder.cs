using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class SemanticDomainBuilder : GeneralModelBuilder<Models.Lexicon_SemanticDomain>
{
    public Func<ProjectDbContext, IEnumerable<Models.Lexicon_SemanticDomain>> GetSemanticDomains = (projectDbContext) =>
    {
        return projectDbContext.Lexicon_SemanticDomains
            .Include(e => e.SemanticDomainMeaningAssociations)
            .OrderBy(c => c.Created)
            .ToList();
    };

    public override IEnumerable<GeneralModel<Models.Lexicon_SemanticDomain>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.Lexicon_SemanticDomain>>();

        var modelItems = GetSemanticDomains(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public static GeneralModel<Models.Lexicon_SemanticDomain> BuildModelSnapshot(Models.Lexicon_SemanticDomain semanticDomain, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(semanticDomain, builderContext.CommonIgnoreProperties);

        if (semanticDomain.SemanticDomainMeaningAssociations.Any())
        {
            var modelSnapshotAssociations = new GeneralListModel<GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>>();
            foreach (var association in semanticDomain.SemanticDomainMeaningAssociations)
            {
                modelSnapshotAssociations.Add(ExtractUsingModelIds<Models.Lexicon_SemanticDomainMeaningAssociation>(
                    association, 
                    builderContext.CommonIgnoreProperties
                ));
            }

            modelSnapshot.AddChild("SemanticDomainMeaningAssociations", modelSnapshotAssociations.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }
}
