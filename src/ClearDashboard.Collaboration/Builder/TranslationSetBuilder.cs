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
        modelSnapshot.AddChild("Translations", translationBuilder.BuildModelSnapshots(translationSet.Id, builderContext).AsModelSnapshotChildrenList());

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
}
