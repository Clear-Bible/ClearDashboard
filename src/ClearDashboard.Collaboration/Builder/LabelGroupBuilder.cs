using System.Data.Entity.Infrastructure;
using System.Text.Json;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LabelGroupBuilder : GeneralModelBuilder<Models.LabelGroup>
{
    public const string LABEL_GROUP_ASSOCIATIONS_CHILD_NAME = "LabelGroupAssociations";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) }
        };

    public Func<ProjectDbContext, IEnumerable<Models.LabelGroup>> GetLabelGroups = (projectDbContext) =>
    {
        return projectDbContext.LabelGroups.OrderBy(l => l.Name).ToList();
    };

    public override IEnumerable<GeneralModel<Models.LabelGroup>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.LabelGroup>>();

        var labelGroupDbModels = GetLabelGroups(builderContext.ProjectDbContext);

        var labelGroupAssociationBuilder = (LabelGroupAssociationBuilder)GetModelBuilder<Models.LabelGroupAssociation>();
        var lgaDbModelsByLabelId = labelGroupAssociationBuilder.GetLabelGroupAssociationsByLabelGroupId(builderContext.ProjectDbContext);

        foreach (var labelGroupDbModel in labelGroupDbModels)
        {
            var modelSnapshot = BuildLabelGroupModelSnapshot(labelGroupDbModel, builderContext);
            if (lgaDbModelsByLabelId.TryGetValue(labelGroupDbModel.Id, out var lgs))
            {
                var labelGroupAssociationModelSnapshots = new GeneralListModel<GeneralModel<Models.LabelGroupAssociation>>();

                foreach (var lg in lgs)
                {
                    var lgModelSnapshot = LabelGroupAssociationBuilder.BuildLabelGroupAssociationModelSnapshot(
                        lg,
                        labelGroupDbModel.Name,
                        lg.Label!.Text!,
                        builderContext);

                    labelGroupAssociationModelSnapshots.Add(lgModelSnapshot);
                }

                modelSnapshot.AddChild(LABEL_GROUP_ASSOCIATIONS_CHILD_NAME, labelGroupAssociationModelSnapshots.AsModelSnapshotChildrenList());
            }
            modelSnapshots.Add(modelSnapshot);
        }

        return modelSnapshots;
    }

    private static GeneralModel<Models.LabelGroup> BuildLabelGroupModelSnapshot(Models.LabelGroup dbModel, BuilderContext builderContext)
    {
        var modelSnapshotProperties = ExtractUsingModelRefs(
            dbModel, 
            builderContext, 
            new List<string>() { "Id" });

        var snapshot = new GeneralModel<Models.LabelGroup>(BuildPropertyRefName(), CalculateLabelGroupRef(dbModel.Name));
        AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

        return snapshot;
    }

    private static string CalculateLabelGroupRef(string labelGroupName)
    {
        return $"LabelGroup_{labelGroupName.ToMD5String()}";
    }

    public override GeneralModel<Models.LabelGroup> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.LabelGroup.Name), out var labelGroupName))
            {
                modelPropertiesTypes.Remove(nameof(Models.Label.Id));
                modelPropertiesTypes.Add(BuildPropertyRefName(), (typeof(string), CalculateLabelGroupRef((string)labelGroupName.value!)));
            }
            else
            {
                throw new PropertyResolutionException($"LabelGroup snapshot does not have Name property value, which is required for Ref calculation.");
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }

    public override void FinalizeTopLevelEntities(List<GeneralModel<Models.LabelGroup>> topLevelEntities)
    {
        for (int i = 0; i < topLevelEntities.Count; i++)
        {
            var changed = false;
            var topLevelEntity = topLevelEntities[i];

            if (topLevelEntity.TryGetChildValue(LABEL_GROUP_ASSOCIATIONS_CHILD_NAME, out var children) && children!.Any() &&
                children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.LabelNoteAssociation>>)))
            {
                var lgaModelSnapshots = (IEnumerable<GeneralModel<Models.LabelGroupAssociation>>)children!;

            }
        }
    }
}
