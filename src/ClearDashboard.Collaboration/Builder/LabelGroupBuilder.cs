using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class LabelGroupBuilder : GeneralModelBuilder<Models.LabelGroup>
{
    public const string LABEL_GROUP_ASSOCIATIONS_CHILD_NAME = "LabelGroupAssociations";

    public const string LABELGROUP_REF_PREFIX = "LabelGroup";
    public const string LABELGROUPASSOCIATION_REF_PREFIX = "LabelGroupLabel";

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

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.LabelGroupAssociation>>> GetLabelGroupAssociationsByLabelGroupId = (projectDbContext) =>
    {
        return projectDbContext.LabelGroupAssociations
            .Include(e => e.Label)
            .Include(e => e.LabelGroup)
            .ToList()
            .GroupBy(e => e.LabelGroupId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    };

    public override IEnumerable<GeneralModel<Models.LabelGroup>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.LabelGroup>>();

        var labelGroupDbModels = GetLabelGroups(builderContext.ProjectDbContext);

        var lgaDbModelsByLabelId = GetLabelGroupAssociationsByLabelGroupId(builderContext.ProjectDbContext);

        foreach (var labelGroupDbModel in labelGroupDbModels)
        {
            var labelGroupRef = EncodeLabelGroupRef(labelGroupDbModel.Name);
            var labelGroup = BuildRefModelSnapshot(
                    labelGroupDbModel,
                    labelGroupRef,
                    GetLabelGroupRef(labelGroupDbModel.Name),
                    null,
                    builderContext);

            if (lgaDbModelsByLabelId.TryGetValue(labelGroupDbModel.Id, out var lgas))
            {
                var labelGroupAssociationModelSnapshots = new GeneralListModel<GeneralModel<Models.LabelGroupAssociation>>();

                foreach (var lga in lgas)
                {
                    var labelRef = LabelBuilder.EncodeLabelRef(lga.Label!.Text!);
                    var lgaModelSnapshot = BuildRefModelSnapshot(
                        lga,
                        GetLabelGroupAssociationRef(lga.LabelGroup!.Name, lga.Label!.Text!),
                        null,
                        new (string, string?, bool)[] { (LABELGROUP_REF_PREFIX, labelGroupRef, true), (LabelBuilder.LABEL_REF_PREFIX, labelRef, true) },
                        builderContext);

                    labelGroupAssociationModelSnapshots.Add(lgaModelSnapshot);
                }

                labelGroup.AddChild(LABEL_GROUP_ASSOCIATIONS_CHILD_NAME, labelGroupAssociationModelSnapshots.AsModelSnapshotChildrenList());
            }
            modelSnapshots.Add(labelGroup);
        }

        return modelSnapshots;
    }

    private static string GetLabelGroupRef(string labelGroupName) => HashPartsToRef(LABELGROUP_REF_PREFIX, labelGroupName);
    private static string GetLabelGroupAssociationRef(string labelGroupName, string labelText) => HashPartsToRef(LABELGROUPASSOCIATION_REF_PREFIX, labelGroupName, labelText);
    private static string EncodeLabelGroupRef(string labelGroupName) => EncodePartsToRef(LABELGROUP_REF_PREFIX, labelGroupName);
    public static string DecodeLabelGroupRef(string labelGroupRef) => DecodeRefToParts(LABELGROUP_REF_PREFIX, labelGroupRef, 1)[0];
}
