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

public class LabelNoteAssociationBuilder : GeneralModelBuilder<Models.LabelNoteAssociation>
{
    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { nameof(Models.Label.Text), typeof(string) }
        };

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.LabelNoteAssociation>>> GetLabelNoteAssociationsByLabelId = (projectDbContext) =>
    {
        return projectDbContext.LabelNoteAssociations
            .Include(e => e.Label)
            .ToList()
            .GroupBy(e => e.LabelId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    };

    public static GeneralModel<Models.LabelNoteAssociation> BuildLabelNoteAssociationModelSnapshot(Models.LabelNoteAssociation dbModel, string labelText, BuilderContext builderContext)
    {
        var modelSnapshotProperties = ExtractUsingModelRefs(
            dbModel,
            builderContext,
            new List<string>() { "Id" });

        modelSnapshotProperties.Remove(nameof(Models.LabelNoteAssociation.LabelId));
        modelSnapshotProperties.Add(nameof(Models.Label.Text), (typeof(string), labelText));

        var snapshot = new GeneralModel<Models.LabelNoteAssociation>(
            BuildPropertyRefName(), 
            CalculateLabelNoteAssociationRef(labelText, dbModel.NoteId));

        AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

        return snapshot;
    }

    private static string CalculateLabelNoteAssociationRef(string labelText, Guid noteId)
    {
        return $"LabelNoteAssociation_{(labelText + noteId.ToString()).ToMD5String()}";
    }

    public override GeneralModel<Models.LabelNoteAssociation> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            // If its serialized as the old style, just load it that way.  We'll
            // need to change it to "Ref" during Label.FinalizeTopLevelEntities
            // since we don't have enough info to do it here (we need Label data)

            GeneralModel<Models.LabelNoteAssociation>? generalModel = new(
                nameof(Models.LabelNoteAssociation.Id),
                (ValueType)modelPropertiesTypes[nameof(Models.LabelNoteAssociation.Id)].value!);

            modelPropertiesTypes.Remove(nameof(Models.LabelNoteAssociation.Id));

            AddPropertyValuesToGeneralModel(generalModel, modelPropertiesTypes);
            return generalModel;
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }
}
