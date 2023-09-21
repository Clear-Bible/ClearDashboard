using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using SIL.Extensions;

namespace ClearDashboard.Collaboration.Builder;

public class TokenBuilder : GeneralModelBuilder<Models.Token>
{
    public const string VERSE_ROW_LOCATION = "VerseRowLocation";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { VERSE_ROW_LOCATION, typeof(string) }
        };

    public static IEnumerable<(Models.Token Token, int? OriginTokenLocationIndex)> OrganizeTokensByOriginTokenLocation(IEnumerable<Models.Token> tokens)
    {
        // Tokens manually changed (e.g. Token Splitting/subword renumbering):
        var tokenIndexes = tokens
            .Where(e => e.OriginTokenLocation != null)
            .ToList()
            .GroupBy(e => e.OriginTokenLocation)
            .SelectMany(g => g.Select((e, index) => (Token: e, Index: index as int?)))
            .ToList();

        var manuallyChangedOriginTokenLocations = tokenIndexes
            .Select(e => e.Token.OriginTokenLocation!)
            .Distinct();

        // Tokens that were 'replaced' by manually created tokens:
        // (Tokens with an EngineTokenId that matches any of the 
        // OriginTokenLocation values from above)
        tokenIndexes.AddRange(tokens
            .Where(e => e.OriginTokenLocation == null)
            .Where(e => manuallyChangedOriginTokenLocations.Contains(e.EngineTokenId))
            .ToList()
            .Select(e => (Token: e, Index: null as int?))
        );

        return tokenIndexes;
    }

    public Func<ProjectDbContext, Guid, IEnumerable<(Models.Token Token, int? OriginTokenLocationIndex)>> GetTokenizedCorpusTokens = 
        (projectDbContext, tokenizedCorpusId) =>
            {
                return OrganizeTokensByOriginTokenLocation(projectDbContext.Tokens
                    .Include(e => e.VerseRow)
                    .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                );
            };

    public static GeneralModel<Models.Token> BuildModelSnapshot((Models.Token Token, int? Index) tokenOriginTokenLocationIndex, BuilderContext builderContext)
    {
        var modelProperties = ExtractUsingModelRefs(
            tokenOriginTokenLocationIndex.Token, 
            builderContext, 
            new List<string>() { "Id", "VerseRowId" });

        modelProperties.Add(VERSE_ROW_LOCATION, (typeof(string), tokenOriginTokenLocationIndex.Token.VerseRow?.BookChapterVerse));

        var refValue = CalculateRef(
            tokenOriginTokenLocationIndex.Token.TokenizedCorpusId,
            tokenOriginTokenLocationIndex.Token.EngineTokenId!,
            tokenOriginTokenLocationIndex.Token.OriginTokenLocation,
            tokenOriginTokenLocationIndex.Index
        );

        var tokenModelSnapshot = new GeneralModel<Models.Token>(BuildPropertyRefName(), refValue);
        GeneralModelBuilder<Models.Alignment>.AddPropertyValuesToGeneralModel(tokenModelSnapshot, modelProperties);

        return tokenModelSnapshot;
    }

    private static string CalculateRef(Guid tokenizedCorpusId, string engineTokenId, string? originTokenLocation, int? index)
    {
        // If a split token, use {OriginTokenLocation}_{Index} (index, when ordered by Subword,
        // within the set of tokens having this OriginTokenLocation).  Else use EngineTokenId
        var identityPropertyValue = (
            tokenizedCorpusId.ToString() +
            originTokenLocation ?? engineTokenId
        ).ToMD5String();

        if (index != null)
        {
            identityPropertyValue += $"_{index}";
        }

        return $"Token_{identityPropertyValue}";
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
            TokenSurfaceText = tokenComponent.SurfaceText!
        };
    }

    public static string BuildTokenLocation(Models.Token token)
    {
        return $"{token.BookNumber:000}{token.ChapterNumber:000}{token.VerseNumber:000}{token.WordNumber:000}{token.SubwordNumber:000}";
    }
}
