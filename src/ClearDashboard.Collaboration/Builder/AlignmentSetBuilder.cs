using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class AlignmentSetBuilder : GeneralModelBuilder<Models.AlignmentSet>
{
    public const string SOURCE_TOKENIZED_CORPUS = "SourceTokenizedCorpus";
    public const string TARGET_TOKENIZED_CORPUS = "TargetTokenizedCorpus";

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { SOURCE_TOKENIZED_CORPUS, typeof(TokenizedCorpusExtra) },
            { TARGET_TOKENIZED_CORPUS, typeof(TokenizedCorpusExtra) },
        };

    public override IEnumerable<GeneralModel<Models.AlignmentSet>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.AlignmentSet>>();

        var alignmentSets = GetAlignmentSets(builderContext.ProjectDbContext);
        foreach (var dbModel in alignmentSets)
        {
            modelSnapshots.Add(BuildModelSnapshot(dbModel, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.AlignmentSet> BuildModelSnapshot(Models.AlignmentSet alignmentSet, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(alignmentSet, builderContext.CommonIgnoreProperties);

        var sourceTokenizedCorpus = alignmentSet.ParallelCorpus!.SourceTokenizedCorpus!;
        var targetTokenizedCorpus = alignmentSet.ParallelCorpus!.TargetTokenizedCorpus!;

        var sourceTokenizedCorpusExtra = new TokenizedCorpusExtra
        {
            Id = sourceTokenizedCorpus!.Id,
            Language = sourceTokenizedCorpus!.Corpus!.Language!,
            Tokenization = sourceTokenizedCorpus!.TokenizationFunction!,
            LastTokenized = sourceTokenizedCorpus!.LastTokenized,
        };

        var targetTokenizedCorpusExtra = new TokenizedCorpusExtra
        {
            Id = targetTokenizedCorpus!.Id,
            Language = targetTokenizedCorpus!.Corpus!.Language!,
            Tokenization = targetTokenizedCorpus!.TokenizationFunction!,
            LastTokenized = targetTokenizedCorpus!.LastTokenized,
        };

        modelSnapshot.Add(SOURCE_TOKENIZED_CORPUS, sourceTokenizedCorpusExtra);
        modelSnapshot.Add(TARGET_TOKENIZED_CORPUS, targetTokenizedCorpusExtra);

        modelSnapshot.AddChild("Alignments", AlignmentBuilder.BuildModelSnapshots(alignmentSet.Id, builderContext).AsModelSnapshotChildrenList());

        return modelSnapshot;
    }

    public static IEnumerable<Models.AlignmentSet> GetAlignmentSets(ProjectDbContext projectDbContext)
    {
        // The only thing we can do with Alignments at present is auto-create them
        // and possibly soft-delete them, grab any manually deleted ones:
        return projectDbContext.AlignmentSets
            .Include(e => e.ParallelCorpus)
                .ThenInclude(e => e!.SourceTokenizedCorpus)
                    .ThenInclude(e => e!.Corpus)
            .Include(e => e.ParallelCorpus)
                .ThenInclude(e => e!.TargetTokenizedCorpus)
                    .ThenInclude(e => e!.Corpus)
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
