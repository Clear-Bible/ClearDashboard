using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Factory;

namespace ClearDashboard.Collaboration.Builder;

public class TokenizedCorpusBuilder : GeneralModelBuilder<Models.TokenizedCorpus>
{
    public const string BOOK_NUMBERS = "BookNumbers";

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BOOK_NUMBERS, typeof(GeneralListModel<string>) }
        };

    public VerseRowBuilder? VerseRowBuilder = null;
    public TokenCompositeBuilder? TokenCompositeBuilder = null;
    public TokenBuilder? TokenBuilder = null;

    public Func<ProjectDbContext, IEnumerable<Models.TokenizedCorpus>> GetTokenizedCorpora = (projectDbContext) =>
    {
        return projectDbContext.TokenizedCorpora
            .Include(tc => tc.Corpus)
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(c => c.Created)
            .ToList();
    };

    public Func<ProjectDbContext, Guid, GeneralListModel<string>> GetBookNumbers = (projectDbContext, tokenizedCorpusId) =>
    {
        return projectDbContext.VerseRows
            .AsNoTrackingWithIdentityResolution()
            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusId)
            .Select(vr => vr.BookChapterVerse!.Substring(0, 3))
            .Distinct()
            .OrderBy(b => b)
            .ToGeneralListModel();
    };

    public override IEnumerable<GeneralModel<Models.TokenizedCorpus>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.TokenizedCorpus>>();

        var modelItems = GetTokenizedCorpora(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshot.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshot;
    }

    public GeneralModel<Models.TokenizedCorpus> BuildModelSnapshot(Models.TokenizedCorpus tokenizedCorpus, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(tokenizedCorpus, builderContext.CommonIgnoreProperties);

        if (tokenizedCorpus.Corpus!.CorpusType != Models.CorpusType.ManuscriptHebrew &&
            tokenizedCorpus.Corpus!.CorpusType != Models.CorpusType.ManuscriptGreek)
        {
            modelSnapshot.Add(BOOK_NUMBERS, GetBookNumbers(builderContext.ProjectDbContext, tokenizedCorpus.Id));

            var verseRowChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.VerseRow)].childName;
            modelSnapshot.AddChild(verseRowChildName, (VerseRowBuilder ?? new VerseRowBuilder())
                .BuildModelSnapshots(tokenizedCorpus.Id, builderContext)
                .AsModelSnapshotChildrenList());
        }

        TokenCompositeBuilder ??= (TokenCompositeBuilder)GetModelBuilder<Models.TokenComposite>();
        var tokenCompositeModelSnapshots = BuildTokenCompositeModelSnapshots(TokenCompositeBuilder, builderContext, tokenizedCorpus.Id);
        if (tokenCompositeModelSnapshots is not null)
        {
            var compositeChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.TokenComposite)].childName;
            modelSnapshot.AddChild(compositeChildName, tokenCompositeModelSnapshots.AsModelSnapshotChildrenList());
        }

        TokenBuilder ??= (TokenBuilder)GetModelBuilder<Models.Token>();
        var tokenModelSnapshots = BuildTokenModelSnapshots(TokenBuilder, builderContext, tokenizedCorpus.Id);
        if (tokenModelSnapshots is not null)
        {
            var tokenChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.Token)].childName;
            modelSnapshot.AddChild(tokenChildName, tokenModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static GeneralListModel<GeneralModel<Models.TokenComposite>>? BuildTokenCompositeModelSnapshots(TokenCompositeBuilder tokenCompositeBuilder, BuilderContext builderContext, Guid tokenizedCorpusId)
    {
        var tokenCompositeTokens = tokenCompositeBuilder.GetTokenizedCorpusCompositeTokens(builderContext.ProjectDbContext, tokenizedCorpusId);

        if (tokenCompositeTokens.Any())
        {
            var compositeModelSnapshots = new GeneralListModel<GeneralModel<Models.TokenComposite>>();

            foreach (var (TokenComposite, Tokens) in tokenCompositeTokens)
            {
                compositeModelSnapshots.Add(TokenCompositeBuilder.BuildModelSnapshot(TokenComposite, Tokens, builderContext));
            }

            return compositeModelSnapshots;
        }

        return null;
    }

    public static GeneralListModel<GeneralModel<Models.Token>>? BuildTokenModelSnapshots(TokenBuilder tokenBuilder, BuilderContext builderContext, Guid tokenizedCorpusId, IEnumerable<string>? engineTokenIdAdditions = null)
    {
        var tokens = tokenBuilder.GetTokenizedCorpusTokens(
            builderContext.ProjectDbContext, 
            tokenizedCorpusId, 
            engineTokenIdAdditions);

        if (tokens.Any())
        {
            var tokenModelSnapshots = new GeneralListModel<GeneralModel<Models.Token>>();

            foreach (var token in tokens)
            {
                tokenModelSnapshots.Add(TokenBuilder.BuildModelSnapshot(token, builderContext));
            }

            return tokenModelSnapshots;
        }

        return null;
    }
}
