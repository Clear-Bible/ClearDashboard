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

    public const string ALIGNMENTS_CHILD_NAME = "Alignments";

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { SOURCE_TOKENIZED_CORPUS, typeof(TokenizedCorpusExtra) },
            { TARGET_TOKENIZED_CORPUS, typeof(TokenizedCorpusExtra) },
        };

    public AlignmentBuilder? AlignmentBuilder = null;

    public Func<ProjectDbContext, IEnumerable<Models.AlignmentSet>> GetAlignmentSets = (projectDbContext) =>
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

    public GeneralModel<Models.AlignmentSet> BuildModelSnapshot(Models.AlignmentSet alignmentSet, BuilderContext builderContext)
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

        var alignmentBuilder = AlignmentBuilder ?? new AlignmentBuilder();
        modelSnapshot.AddChild(ALIGNMENTS_CHILD_NAME, alignmentBuilder.BuildModelSnapshots(alignmentSet.Id, builderContext).AsModelSnapshotChildrenList());

        return modelSnapshot;
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

    public override void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        foreach (var parentSnapshot in projectSnapshot.GetGeneralModelList<Models.AlignmentSet>())
        {
            if (parentSnapshot.TryGetGuidPropertyValue(nameof(Models.AlignmentSet.ParallelCorpusId), out var parallelCorpusId) &&  
                parentSnapshot.TryGetChildValue(ALIGNMENTS_CHILD_NAME, out var children) &&
                children!.Any() &&
                children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Alignment>>)))
            {
                var alignmentSnapshots = (IEnumerable<GeneralModel<Models.Alignment>>)children!;
                foreach (var alignmentSnapshot in alignmentSnapshots)
                {
                    if (!alignmentSnapshot.TryGetPropertyValue(AlignmentBuilder.SOURCE_TOKEN_DELETED, out var sourceTokenDeleted))
                    {
                        // TODO:  find token in snapshot and look at DELETED property to see if null or not
                        alignmentSnapshot.Add(AlignmentBuilder.SOURCE_TOKEN_DELETED, false, typeof(bool));
                    }

                    if (!alignmentSnapshot.TryGetPropertyValue(AlignmentBuilder.TARGET_TOKEN_DELETED, out var targetTokenDeleted))
                    {
                        // TODO:  find token in snapshot and look at DELETED property to see if null or not
                        alignmentSnapshot.Add(AlignmentBuilder.TARGET_TOKEN_DELETED, false, typeof(bool));
                    }

                }
            }
        }
    }
}
