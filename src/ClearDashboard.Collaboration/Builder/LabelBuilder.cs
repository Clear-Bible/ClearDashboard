using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LabelBuilder : GeneralModelBuilder<Models.Label>
{
    public const string LABEL_NOTE_ASSOCIATIONS_CHILD_NAME = "LabelNoteAssociations";

    public const string LABEL_REF_PREFIX = "Label";
    public const string LABELNOTEASSOCIATION_REF_PREFIX = "LabelNote";

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

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.LabelNoteAssociation>>> GetLabelNoteAssociationsByLabelId = (projectDbContext) =>
    {
        return projectDbContext.LabelNoteAssociations
            .Include(e => e.Label)
            .ToList()
            .GroupBy(e => e.LabelId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    };

    public override IEnumerable<GeneralModel<Models.Label>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Label>>();

        var labelDbModels = GetLabels(builderContext.ProjectDbContext);
        var lnaDbModelsByLabelId = GetLabelNoteAssociationsByLabelId(builderContext.ProjectDbContext);

        foreach (var labelDbModel in labelDbModels)
        {
            var labelRef = EncodeLabelRef(labelDbModel.Text!);
            var label = BuildRefModelSnapshot(
                    labelDbModel,
                    labelRef,
                    GetLabelRef(labelDbModel.Text!),
                    null,
                    builderContext);

            if (lnaDbModelsByLabelId.TryGetValue(labelDbModel.Id, out var lnas))
            {
                var labelNoteAssociationModelSnapshots = new GeneralListModel<GeneralModel<Models.LabelNoteAssociation>>();

                foreach (var lna in lnas)
                {
                    var lgaModelSnapshot = BuildRefModelSnapshot(
                        lna,
                        GetLabelNoteAssociationRef(lna.Label!.Text!, lna.NoteId),
                        null,
                        new (string, string?, bool)[] { (LABEL_REF_PREFIX, labelRef, true) },
                        builderContext);

                    labelNoteAssociationModelSnapshots.Add(lgaModelSnapshot);
                }

                label.AddChild(LABEL_NOTE_ASSOCIATIONS_CHILD_NAME, labelNoteAssociationModelSnapshots.AsModelSnapshotChildrenList());
            }
            modelSnapshots.Add(label);
        }

        return modelSnapshots;
    }

    public static string GetLabelRef(string labelText) => HashPartsToRef(LABEL_REF_PREFIX, labelText);
    private static string GetLabelNoteAssociationRef(string labelName, Guid noteId) => HashPartsToRef(LABELNOTEASSOCIATION_REF_PREFIX, labelName, noteId.ToString());
    public static string EncodeLabelRef(string labelText) => EncodePartsToRef(LABEL_REF_PREFIX, labelText);
    public static string DecodeLabelRef(string labelRef) => DecodeRefToParts(LABEL_REF_PREFIX, labelRef, 1)[0];

    public override GeneralModel<Models.Label> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        // If the entity being deserialized has the older "Id" identity key,
        // convert it to the new system-neutral "Ref" key:
        if (!modelPropertiesTypes.ContainsKey(IdentityKey))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.Label.Text), out var labelText))
            {
                modelPropertiesTypes.Remove(nameof(Models.Label.Id));
                modelPropertiesTypes.Add(BuildPropertyRefName(), (typeof(string), EncodeLabelRef((string)labelText.value!)));
            }
            else
            {
                throw new PropertyResolutionException($"Label snapshot does not have Text property value, which is required for Ref calculation.");
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }

    public override void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        foreach (var labelSnapshot in projectSnapshot.GetGeneralModelList<Models.Label>())
        {
            UpdateLabelNoteAssociationChildren(labelSnapshot);
        }
    }

    private static void UpdateLabelNoteAssociationChildren(GeneralModel<Models.Label> parentSnapshot)
    {
        if (parentSnapshot.TryGetChildValue(LABEL_NOTE_ASSOCIATIONS_CHILD_NAME, out var children) &&
            children!.Any() &&
            children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.LabelNoteAssociation>>)))
        {
            var childSnapshotsExisting = (IEnumerable<GeneralModel<Models.LabelNoteAssociation>>)children!;
            if (!childSnapshotsExisting.Any(e => e.IdentityKey == nameof(Models.LabelNoteAssociation.Id)))
            {
                // If the serialized Meaning was already the "Ref" form, all we need to do
                // here is propagate the Lexeme Ref value so that the handler
                // can find the exact meaning for update or delete:

                //foreach (var childSnapshot in childSnapshotsExisting)
                //{
                //    childSnapshot.Add(BuildPropertyRefName(LEXEME_REF_PREFIX), (string)parentSnapshot.GetId(), typeof(string));
                //    UpdateTranslationChildren(childSnapshot, (string)parentSnapshot.GetId());
                //}
                return;
            }

            if (!parentSnapshot.TryGetStringPropertyValue(nameof(Models.Label.Text), out var labelText))
            {
                throw new PropertyResolutionException($"Label snapshot does not have Text property value, which is required for creating LabelNoteAssociation Ref values in FinalizeTopLevelEntities.");
            }

            var modelSnapshotsNew = new GeneralListModel<GeneralModel<Models.LabelNoteAssociation>>();
            foreach (var modelSnapshotExisting in childSnapshotsExisting)
            {
                GeneralModel<LabelNoteAssociation>? childSnapshotToAdd = null;

                if (modelSnapshotExisting.IdentityKey == nameof(Models.LabelNoteAssociation.Id))
                {
                    if (!modelSnapshotExisting.TryGetGuidPropertyValue(nameof(Models.LabelNoteAssociation.NoteId), out var noteId))
                    {
                        throw new PropertyResolutionException($"Lexicon_Meaning snapshot does not have both Text and Language property values, which are required for creating MeaningRef values in FinalizeTopLevelEntities.");
                    }

                    var modelPropertiesTypes = modelSnapshotExisting.ModelPropertiesTypes;

                    modelPropertiesTypes.Remove(nameof(Models.LabelNoteAssociation.Id));
                    modelPropertiesTypes.Remove(nameof(Models.LabelNoteAssociation.LabelId));
                    modelPropertiesTypes.Add(BuildPropertyRefName(LABEL_REF_PREFIX), (typeof(string), labelText));

                    childSnapshotToAdd = new GeneralModel<Models.LabelNoteAssociation>(
                        BuildPropertyRefName(),
                        GetLabelNoteAssociationRef(labelText, noteId));

                    AddPropertyValuesToGeneralModel(childSnapshotToAdd, modelPropertiesTypes);
                }

                childSnapshotToAdd ??= modelSnapshotExisting;
                modelSnapshotsNew.Add(childSnapshotToAdd);
            }

            parentSnapshot.ReplaceChildrenForKey(LABEL_NOTE_ASSOCIATIONS_CHILD_NAME, modelSnapshotsNew.AsModelSnapshotChildrenList());
        }
    }
}