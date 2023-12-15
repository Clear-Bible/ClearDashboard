using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class TranslationSetBuilder : GeneralModelBuilder<Models.TranslationSet>
{
    public const string SOURCE_TOKENIZED_CORPUS = "SourceTokenizedCorpus";

    public const string TRANSLATIONS_CHILD_NAME = "Translations";

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { SOURCE_TOKENIZED_CORPUS, typeof(TokenizedCorpusExtra) }
        };

    public TranslationBuilder? TranslationBuilder = null;

    public Func<ProjectDbContext, IEnumerable<Models.TranslationSet>> GetTranslationSets = (projectDbContext) =>
    {
        return projectDbContext.TranslationSets
            .Include(e => e.ParallelCorpus)
                .ThenInclude(e => e!.SourceTokenizedCorpus)
            .OrderBy(c => c.Created)
            .ToList();
    };

    public override IEnumerable<GeneralModel<Models.TranslationSet>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.TranslationSet>>();

        var modelItems = GetTranslationSets(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public GeneralModel<Models.TranslationSet> BuildModelSnapshot(Models.TranslationSet translationSet, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(translationSet, builderContext.CommonIgnoreProperties);

        var sourceTokenizedCorpus = translationSet.ParallelCorpus!.SourceTokenizedCorpus!;

        var sourceTokenizedCorpusExtra = new TokenizedCorpusExtra
        {
            Id = sourceTokenizedCorpus!.Id,
            Language = sourceTokenizedCorpus!.Corpus!.Language!,
            Tokenization = sourceTokenizedCorpus!.TokenizationFunction!,
            LastTokenized = sourceTokenizedCorpus!.LastTokenized,
        };

        modelSnapshot.Add(SOURCE_TOKENIZED_CORPUS, sourceTokenizedCorpusExtra);

        var translationBuilder = TranslationBuilder ?? new TranslationBuilder();
        modelSnapshot.AddChild(TRANSLATIONS_CHILD_NAME, translationBuilder.BuildModelSnapshots(translationSet.Id, builderContext).AsModelSnapshotChildrenList());

        return modelSnapshot;
    }

    public static TranslationRef BuildTranslationRef(
        Models.Translation translation,
        BuilderContext builderContext)
    {
        return new TranslationRef
        {
            TranslationSetId = translation.TranslationSetId,
            SourceTokenRef = TokenBuilder.BuildTokenRef(translation.SourceTokenComponent!, builderContext),
        };
    }

    public override void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        foreach (var parentSnapshot in projectSnapshot.GetGeneralModelList<Models.TranslationSet>())
        {
            if (parentSnapshot.TryGetGuidPropertyValue(nameof(Models.TranslationSet.ParallelCorpusId), out var parallelCorpusId) &&
                parentSnapshot.TryGetChildValue(TRANSLATIONS_CHILD_NAME, out var children) &&
                children!.Any() &&
                children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<Models.Translation>>)))
            {
                var translationSnapshots = (IEnumerable<GeneralModel<Models.Translation>>)children!;
                foreach (var translationSnapshot in translationSnapshots)
                {
                    if (!translationSnapshot.TryGetPropertyValue(TranslationBuilder.SOURCE_TOKEN_DELETED, out var sourceTokenDeleted))
                    {
                        // TODO:  find token in snapshot and look at DELETED property to see if null or not
                        translationSnapshot.Add(TranslationBuilder.SOURCE_TOKEN_DELETED, false, typeof(bool));
                    }
                }
            }
        }
    }
}
