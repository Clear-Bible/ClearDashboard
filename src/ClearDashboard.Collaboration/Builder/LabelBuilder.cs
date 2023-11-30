using System.Data.Entity.Infrastructure;
using System.Text.Json;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LabelBuilder : GeneralModelBuilder<Models.Label>
{
    public const string LABEL_NOTE_ASSOCIATIONS_CHILD_NAME = "LabelNoteAssociations";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) }
        };

    public Func<ProjectDbContext, IEnumerable<Models.Label>> GetLabels = (projectDbContext) =>
    {
        return projectDbContext.Labels.OrderBy(l => l.Text).ToList();
    };

    public Func<LabelNoteAssociationBuilder> GetLabelNoteAssociationBuilder = () =>
    {
        return (LabelNoteAssociationBuilder)GeneralModelBuilder.GetModelBuilder<Models.LabelNoteAssociation>();
    };

    public override IEnumerable<GeneralModel<Models.Label>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Label>>();

        var labelDbModels = GetLabels(builderContext.ProjectDbContext);
        var lnaDbModelsByLabelId = GetLabelNoteAssociationBuilder().GetLabelNoteAssociationsByLabelId(builderContext.ProjectDbContext);

        foreach (var labelDbModel in labelDbModels)
        {
            var modelSnapshot = BuildLabelModelSnapshot(labelDbModel, builderContext);
            if (lnaDbModelsByLabelId.TryGetValue(labelDbModel.Id, out var lns))
            {
                var labelNoteAssociationModelSnapshots = new GeneralListModel<GeneralModel<Models.LabelNoteAssociation>>();

                foreach (var ln in lns)
                {
                    var lnModelSnapshot = LabelNoteAssociationBuilder.BuildLabelNoteAssociationModelSnapshot(
                        ln, 
                        labelDbModel.Text!, 
                        builderContext);

                    labelNoteAssociationModelSnapshots.Add(lnModelSnapshot);
                }

                modelSnapshot.AddChild(LABEL_NOTE_ASSOCIATIONS_CHILD_NAME, labelNoteAssociationModelSnapshots.AsModelSnapshotChildrenList());
            }
            modelSnapshots.Add(modelSnapshot);
        }

        return modelSnapshots;
    }

    private static GeneralModel<Models.Label> BuildLabelModelSnapshot(Models.Label dbModel, BuilderContext builderContext)
    {
        var modelSnapshotProperties = ExtractUsingModelRefs(
            dbModel, 
            builderContext, 
            new List<string>() { "Id" });

        var snapshot = new GeneralModel<Models.Label>(BuildPropertyRefName(), CalculateLabelRef(dbModel.Text!));
        AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

        return snapshot;
    }

    private static string CalculateLabelRef(string labelText)
    {
        return $"Label_{labelText.ToMD5String()}";
    }

    public override GeneralModel<Models.Label> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.Label.Text), out var labelText))
            {
                modelPropertiesTypes.Remove(nameof(Models.Label.Id));
                modelPropertiesTypes.Add(BuildPropertyRefName(), (typeof(string), CalculateLabelRef((string)labelText.value!)));
            }
            else
            {
                throw new PropertyResolutionException($"Label snapshot does not have Text property value, which is required for Ref calculation.");
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }

    public override void FinalizeTopLevelEntities(List<GeneralModel<Models.Label>> topLevelEntities)
    {
        for (int i = 0; i < topLevelEntities.Count; i++)
        {
            var changed = false;
            var topLevelEntity = topLevelEntities[i];

            if (topLevelEntity.TryGetChildValue(LABEL_NOTE_ASSOCIATIONS_CHILD_NAME, out var children) && children!.Any() &&
                children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.LabelNoteAssociation>>)))
            {
                var lnaModelSnapshots = (IEnumerable<GeneralModel<Models.LabelNoteAssociation>>)children!;

            }
        }
    }
}
