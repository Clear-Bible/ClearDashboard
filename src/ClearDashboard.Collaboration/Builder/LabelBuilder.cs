using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LabelBuilder : GeneralModelBuilder<Models.Label>
{ 
    public override IEnumerable<GeneralModel<Models.Label>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Label>>();

        var labelDbModels = GetLabels(builderContext.ProjectDbContext);
        var lnaDbModelsByLabelId = GetLabelNoteAssociationsByLabelId(builderContext.ProjectDbContext);

        foreach (var labelDbModel in labelDbModels)
        {
            var modelSnapshot = ExtractUsingModelIds(labelDbModel);
            if (lnaDbModelsByLabelId.TryGetValue(labelDbModel.Id, out var lns))
            {
                var labelNoteAssociationModelSnapshots = new GeneralListModel<GeneralModel<Models.LabelNoteAssociation>>();
                foreach (var ln in lns)
                {
                    labelNoteAssociationModelSnapshots.Add(GeneralModelBuilder<Models.LabelNoteAssociation>.ExtractUsingModelIds(ln));
                }
                modelSnapshot.AddChild("LabelNoteAssociations", labelNoteAssociationModelSnapshots.AsModelSnapshotChildrenList());
            }
            modelSnapshots.Add(modelSnapshot);
        }

        return modelSnapshots;
    }

    public static IEnumerable<Models.Label> GetLabels(ProjectDbContext projectDbContext)
    {
        return projectDbContext.Labels.OrderBy(l => l.Text).ToList();
    }

    public static Dictionary<Guid, IEnumerable<Models.LabelNoteAssociation>> GetLabelNoteAssociationsByLabelId(ProjectDbContext projectDbContext)
    {
        return projectDbContext.LabelNoteAssociations
            .ToList()
            .GroupBy(e => e.LabelId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    }
}
