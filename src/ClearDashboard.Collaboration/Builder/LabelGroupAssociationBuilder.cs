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

public class LabelGroupAssociationBuilder : GeneralModelBuilder<Models.LabelGroupAssociation>
{
    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { nameof(Models.LabelGroup.Name), typeof(string) },
            { nameof(Models.Label.Text), typeof(string) }
        };

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.LabelGroupAssociation>>> GetLabelGroupAssociationsByLabelGroupId = (projectDbContext) =>
    {
        return projectDbContext.LabelGroupAssociations
            .Include(e => e.Label)
            .Include(e => e.LabelGroup)
            .ToList()
            .GroupBy(e => e.LabelGroupId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    };

    public static GeneralModel<Models.LabelGroupAssociation> BuildLabelGroupAssociationModelSnapshot(Models.LabelGroupAssociation dbModel, string labelGroupName, string labelText, BuilderContext builderContext)
    {
        var modelSnapshotProperties = ExtractUsingModelRefs(
            dbModel,
            builderContext,
            new List<string>() { "Id" });

        modelSnapshotProperties.Remove(nameof(Models.LabelGroupAssociation.LabelGroupId));
        modelSnapshotProperties.Add(nameof(Models.LabelGroup.Name), (typeof(string), labelGroupName));

        modelSnapshotProperties.Remove(nameof(Models.LabelGroupAssociation.LabelId));
        modelSnapshotProperties.Add(nameof(Models.Label.Text), (typeof(string), labelText));

        var snapshot = new GeneralModel<Models.LabelGroupAssociation>(BuildPropertyRefName(), CalculateLabelGroupAssociationRef(labelGroupName, labelText));
        AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

        return snapshot;
    }

    private static string CalculateLabelGroupAssociationRef(string labelGroupName, string labelText)
    {
        return $"LabelGroupAssociation_{(labelGroupName + labelText).ToMD5String()}";
    }

    public override GeneralModel<Models.LabelGroupAssociation> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            // If its serialized as the old style, just load it that way.  We'll
            // need to change it to "Ref" during Label.FinalizeTopLevelEntities
            // since we don't have enough info to do it here (we need Label data)

            GeneralModel<Models.LabelGroupAssociation>? generalModel = new(
                nameof(Models.LabelGroupAssociation.Id),
                (ValueType)modelPropertiesTypes[nameof(Models.LabelGroupAssociation.Id)].value!);

            modelPropertiesTypes.Remove(nameof(Models.LabelGroupAssociation.Id));

            AddPropertyValuesToGeneralModel(generalModel, modelPropertiesTypes);
            return generalModel;
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }
}
