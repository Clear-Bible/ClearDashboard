using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class AlignmentBuilder : GeneralModelBuilder<Models.Alignment>
{
    public const string SOURCE_TOKEN_LOCATION = "SourceTokenLocation";
    public const string SOURCE_TOKEN_DELETED = "SourceTokenDeleted";
    public const string TARGET_TOKEN_LOCATION = "TargetTokenLocation";
    public const string TARGET_TOKEN_DELETED = "TargetTokenDeleted";
    public const string BOOK_CHAPTER_LOCATION = "Location";

    public override string IdentityKey => BuildPropertyRefName();
    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { SOURCE_TOKEN_LOCATION, typeof(string) },
            { SOURCE_TOKEN_DELETED, typeof(bool) },
            { TARGET_TOKEN_LOCATION, typeof(string) },
            { TARGET_TOKEN_DELETED, typeof(bool) },
            { BOOK_CHAPTER_LOCATION, typeof(string) }
        };

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.Alignment alignment, Models.Token leadingToken)>> GetAlignments = (projectDbContext, alignmentSetId) =>
    {
        var alignments = projectDbContext.Alignments
            .Include(e => e.SourceTokenComponent!)
                .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
            .Include(e => e.TargetTokenComponent!)
            .Where(e => e.AlignmentSetId == alignmentSetId)
            .ToList()
            .Select(e => (
                alignment: e,
                leadingToken: e.SourceTokenComponent as Models.Token ??
                             (e.SourceTokenComponent as Models.TokenComposite)!.Tokens.First()
            ))
            .OrderBy(e => e.leadingToken.EngineTokenId);

        // Including 'leadingToken' so that even if an Alignment has a composite
        // as its SourceTokenComponent, we can pull book and chapter numbers for
        // grouping during serialization
        return alignments;
    };

    // We serialize this in groups where the AlignmentSetId is in the heading, so don't include it here:
    public override IEnumerable<string> NoSerializePropertyNames => new[] { nameof(Models.Alignment.AlignmentSetId) };

    public GeneralListModel<GeneralModel<Models.Alignment>> BuildModelSnapshots(Guid alignmentSetId, BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.Alignment>>();

        var alignments = GetAlignments(builderContext.ProjectDbContext, alignmentSetId);
        foreach (var alignment in alignments)
        {
            modelSnapshots.Add(BuildModelSnapshot(alignment, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.Alignment> BuildModelSnapshot((Models.Alignment alignment, Models.Token leadingToken) alignment, BuilderContext builderContext)
    {
        var modelProperties = ExtractUsingModelRefs(alignment.alignment, builderContext, new List<string>() { "Id" });

        // FIXME:  enhance GeneralModelBuilder to use propertyConverter delegates
        // so that it produces "SourceTokenLocation", "TargetTokenLocation" itself
        // (it has no way of knowing that AlignmentSet already specifies (via
        // parallel corpus) which is the SourceTokenizedCorpusId and which is
        // the TargetTokenizedCorpusId).  
        var sourceTokenComponentRef = modelProperties["SourceTokenComponentRef"];
        var targetTokenComponentRef = modelProperties["TargetTokenComponentRef"];

        modelProperties.Remove("SourceTokenComponentRef");
        modelProperties.Remove("TargetTokenComponentRef");

        modelProperties.Add(SOURCE_TOKEN_LOCATION,
            (typeof(string), ((TokenRef)sourceTokenComponentRef.value!).TokenLocation));
        modelProperties.Add(SOURCE_TOKEN_DELETED,
            (typeof(bool), ((TokenRef)sourceTokenComponentRef.value!).TokenDeleted));
        modelProperties.Add(TARGET_TOKEN_LOCATION,
            (typeof(string), ((TokenRef)targetTokenComponentRef.value!).TokenLocation));
        modelProperties.Add(TARGET_TOKEN_DELETED,
            (typeof(bool), ((TokenRef)targetTokenComponentRef.value!).TokenDeleted));
        modelProperties.Add(BOOK_CHAPTER_LOCATION,
            (typeof(string), $"{alignment.leadingToken.BookNumber:000}{alignment.leadingToken.ChapterNumber:000}"));

        var identityPropertyValue = (
            alignment.alignment.AlignmentSetId.ToString() +
            modelProperties[SOURCE_TOKEN_LOCATION] +
            modelProperties[TARGET_TOKEN_LOCATION]
        ).ToMD5String();

        var alignmentModelSnapshot = new GeneralModel<Models.Alignment>(BuildPropertyRefName(), $"Alignment_{identityPropertyValue}");
        GeneralModelBuilder<Models.Alignment>.AddPropertyValuesToGeneralModel(alignmentModelSnapshot, modelProperties);

        return alignmentModelSnapshot;
    }

    public static void SaveAlignments(
        GeneralModel alignmentSet,
        GeneralListModel<GeneralModel<Models.Alignment>> alignmentSnapshots,
        string childPath,
        JsonSerializerOptions options,
        CancellationToken cancellationToken)
    {
        var alignmentsByLocation = alignmentSnapshots
            .GroupBy(e => (string)e[AlignmentBuilder.BOOK_CHAPTER_LOCATION]!)
            .ToDictionary(g => g.Key, g => g
                .Select(e => e)
                .OrderBy(e => (string)e[AlignmentBuilder.SOURCE_TOKEN_LOCATION]!)
                .ThenBy(e => (string)e[AlignmentBuilder.TARGET_TOKEN_LOCATION]!)
                .ToGeneralListModel<GeneralModel<Models.Alignment>>())
            .OrderBy(kvp => kvp.Key);

        foreach (var alignmentsForLocation in alignmentsByLocation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Instead of the more general GeneralModelJsonConverter, this will use the
            // more specific AlignmentGroupJsonConverter:
            var alignmentGroup = new AlignmentGroup()
            {
                AlignmentSetId = (Guid)alignmentSet.GetId(),
                SourceTokenizedCorpus = (TokenizedCorpusExtra)alignmentSet[AlignmentSetBuilder.SOURCE_TOKENIZED_CORPUS]!,
                TargetTokenizedCorpus = (TokenizedCorpusExtra)alignmentSet[AlignmentSetBuilder.TARGET_TOKENIZED_CORPUS]!,
                Location = alignmentsForLocation.Key,
                Items = alignmentsForLocation.Value
            };
            var serializedChildModelSnapshot = JsonSerializer.Serialize<AlignmentGroup>(
                alignmentGroup,
                options);
            File.WriteAllText(
                Path.Combine(
                    childPath,
                    string.Format(ProjectSnapshotFactoryCommon.AlignmentsByLocationFileNameTemplate,
                        alignmentSet.GetId(),
                        alignmentsForLocation.Key)),
                serializedChildModelSnapshot);
        }
    }
}
