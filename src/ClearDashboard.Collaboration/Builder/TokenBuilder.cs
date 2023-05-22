using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Collaboration.Builder;

public class TokenBuilder : GeneralModelBuilder<Models.TokenComposite>
{
    public const string VERSE_ROW_LOCATION = "VerseRowLocation";
    public const string TOKEN_LOCATIONS = "TokenLocations";

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { VERSE_ROW_LOCATION, typeof(string) },
            { TOKEN_LOCATIONS, typeof(GeneralListModel<string>) }
        };

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.TokenComposite TokenComposite, IEnumerable<Models.Token> Tokens)>> GetTokenizedCorpusCompositeTokens = 
        (projectDbContext, tokenizedCorpusId) =>
            {
                return projectDbContext.TokenComposites
                    .Include(tc => tc.TokenCompositeTokenAssociations)
                        .ThenInclude(ta => ta.Token)
                    .Include(tc => tc.VerseRow)
                    .Where(tc => tc.ParallelCorpusId == null)
                    .Where(tc => tc.TokenizedCorpusId == tokenizedCorpusId)
                    .ToList()
                    .Select(tc => (TokenComposite: tc, Tokens: tc.Tokens.ToList().AsEnumerable()))
                    .AsEnumerable();
            };

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.TokenComposite TokenComposite, IEnumerable<Models.Token> Tokens)>> GetParallelCorpusCompositeTokens =
        (projectDbContext, parallelCorpusId) =>
            {
                return projectDbContext.TokenComposites
                    .Include(tc => tc.TokenCompositeTokenAssociations)
                    .ThenInclude(ta => ta.Token)
                    .Where(tc => tc.ParallelCorpusId == parallelCorpusId)
                    .ToList()
                    .Select(tc => (TokenComposite: tc, Tokens: tc.Tokens.ToList().AsEnumerable()))
                    .AsEnumerable();
            };

    public static GeneralModel<Models.TokenComposite> BuildModelSnapshot(Models.TokenComposite tokenComposite, IEnumerable<Models.Token> childTokens, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(
            tokenComposite,
            new List<string>() { "VerseRowId" });

        modelSnapshot.Add(VERSE_ROW_LOCATION, tokenComposite.VerseRow?.BookChapterVerse, typeof(string));
        modelSnapshot.Add(TOKEN_LOCATIONS, childTokens.Select(t => BuildTokenLocation(t)).ToGeneralListModel<string>());

        return modelSnapshot;
    }

    public static TokenRef BuildTokenRef(
        Models.TokenComponent tokenComponent,
        BuilderContext builderContext)
    {
        return new TokenRef
        {
            IsComposite = (tokenComponent is Models.TokenComposite),
            TokenizedCorpusId = tokenComponent.TokenizedCorpusId,
            TokenLocation = tokenComponent.EngineTokenId!,
        };
    }

    public static string BuildTokenLocation(Models.Token token)
    {
        return $"{token.BookNumber:000}{token.ChapterNumber:000}{token.VerseNumber:000}{token.WordNumber:000}{token.SubwordNumber:000}";
    }
}
