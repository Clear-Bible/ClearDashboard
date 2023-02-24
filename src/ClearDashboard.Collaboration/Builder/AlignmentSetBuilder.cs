using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class AlignmentSetBuilder : GeneralModelBuilder<Models.AlignmentSet>
{
    public static IEnumerable<GeneralModel<Models.AlignmentSet>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.AlignmentSet>>();

        var dbModels = GetAlignmentSets(builderContext.ProjectDbContext);
        foreach (var dbModel in dbModels)
        {
            modelSnapshots.Add(BuildModelSnapshot(dbModel, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.AlignmentSet> BuildModelSnapshot(Models.AlignmentSet dbModel, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(dbModel, builderContext.CommonIgnoreProperties);

        if (dbModel.Alignments.Any())
        {
            var alignmentModelSnapshots = new GeneralListModel<GeneralModel<Models.Alignment>>();
            foreach (var alignment in dbModel.Alignments)
            {
                var modelProperties = GeneralModelBuilder<Models.Alignment>.ExtractUsingModelRefs(alignment, builderContext, new List<string>() { "Id" });

                // FIXME:  enhance GeneralModelBuilder to use propertyConverter delegates
                // so that it produce "SourceTokenLocation", "TargetTokenLocation" itself
                // (it has no way of knowing that AlignmentSet already specifies (via
                // parallel corpus) which is the SourceTokenizedCorpusId and which is
                // the TargetTokenizedCorpusId).  
                var sourceTokenComponentRef = modelProperties["SourceTokenComponentRef"];
                var targetTokenComponentRef = modelProperties["TargetTokenComponentRef"];

                modelProperties.Remove("SourceTokenComponentRef");
                modelProperties.Remove("TargetTokenComponentRef");

                modelProperties.Add("SourceTokenComponentLocation",
                    (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenLocation));
                modelProperties.Add("TargetTokenComponentLocation",
                    (typeof(string), ((TokenRef)targetTokenComponentRef.value!).TokenLocation));

                var identityPropertyValue = (
                    alignment.AlignmentSetId.ToString() +
                    modelProperties["SourceTokenComponentLocation"]
                ).ToMD5String();

                var alignmentModelSnapshot = new GeneralModel<Models.Alignment>(BuildPropertyRefName(), $"Alignment_{identityPropertyValue}");
                GeneralModelBuilder<Models.Alignment>.AddPropertyValuesToGenericModel(alignmentModelSnapshot, modelProperties);

                alignmentModelSnapshots.Add(alignmentModelSnapshot);
            }
            modelSnapshot.AddChild("Alignments", alignmentModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static IEnumerable<Models.AlignmentSet> GetAlignmentSets(ProjectDbContext projectDbContext)
    {
        // The only thing we can do with Alignments at present is auto-create them
        // and possibly soft-delete them, grab any manually deleted ones:
        return projectDbContext.AlignmentSets
            .Include(ast => ast.Alignments.Where(a => a.Deleted != null)).ThenInclude(a => a.SourceTokenComponent)
            .Include(ast => ast.Alignments.Where(a => a.Deleted != null)).ThenInclude(a => a.TargetTokenComponent)
            .OrderBy(c => c.Created)
            .ToList();
    }

    public static AlignmentRef BuildAlignmentRef(
        Models.Alignment alignment,
        BuilderContext builderContext)
    {
        //var alignmentSetIndex = builderContext.GetIdToIndexValue(nameof(Models.AlignmentSet), alignment.AlignmentSetId);

        return new AlignmentRef
        {
            AlignmentSetId = alignment.AlignmentSetId,
            SourceTokenRef = TokenBuilder.BuildTokenRef(alignment.SourceTokenComponent!, builderContext),
            TargetTokenRef = TokenBuilder.BuildTokenRef(alignment.TargetTokenComponent!, builderContext)
        };
    }
}
