using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using SIL.Extensions;
using System.Linq;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class TokenBuilder : GeneralModelBuilder<Models.Token>
{
    public const string VERSE_ROW_LOCATION = "VerseRowLocation";
    public const string GRAMMAR_SHORT_NAME = "GrammerShortName";

    public override string IdentityKey => BuildPropertyRefName();

    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
        new Dictionary<string, Type>()
        {
            { BuildPropertyRefName(), typeof(string) },
            { VERSE_ROW_LOCATION, typeof(string) },
			{ GRAMMAR_SHORT_NAME, typeof(string) }
		};

    public static IEnumerable<(Models.Token Token, int? OriginTokenLocationIndex)> OrganizeTokensByOriginTokenLocation(IEnumerable<Models.Token> tokens, IEnumerable<string>? engineTokenIdAdditions = null)
    {
        // Tokens manually changed (e.g. Token Splitting/subword renumbering):
        var originTokenLocationGroups = tokens
            .Where(e => e.OriginTokenLocation != null)
            .ToList()
            .GroupBy(e => e.OriginTokenLocation);

        var tokenIndexes = originTokenLocationGroups
            .Where(g => g.Count() > 1)
            .SelectMany(g => g
                .OrderBy(e => e.OriginTokenLocation)
                .OrderBy(e => e.EngineTokenId)
                .Select((e, index) => (Token: e, Index: index as int?)))
            .ToList();

        tokenIndexes.AddRange(originTokenLocationGroups
            .Where(g => g.Count() == 1)
            .Select(g => (Token: g.First(), Index: null as int?))
            .ToList());

        var manuallyChangedOriginTokenLocations = tokenIndexes
            .Select(e => e.Token.OriginTokenLocation!)
            .Distinct();

        // Tokens that were 'replaced' by manually created tokens:
        // (Tokens with an EngineTokenId that matches any of the 
        // OriginTokenLocation values from above)
        if (manuallyChangedOriginTokenLocations.Any())
        {
            tokenIndexes.AddRange(tokens
                .Where(e => e.OriginTokenLocation == null)
                .Where(e => manuallyChangedOriginTokenLocations.Contains(e.EngineTokenId))
                .ToList()
                .Select(e => (Token: e, Index: null as int?))
            );
        }

        // Tokens not yet included in tokenIndexes that were soft deleted:
        tokenIndexes.AddRange(tokens
            .Where(e => e.Deleted != null)
            .Where(e => !tokenIndexes.Select(e => e.Token.Id).Contains(e.Id))
            .ToList()
            .Select(e => (Token: e, Index: null as int?))
        );

        if (engineTokenIdAdditions is not null)
        {
            // Tokens not yet included in tokenIndexes that were soft deleted:
            tokenIndexes.AddRange(tokens
                .Where(e => engineTokenIdAdditions.Contains(e.EngineTokenId))
                .Where(e => !tokenIndexes.Select(e => e.Token.Id).Contains(e.Id))
                .ToList()
                .Select(e => (Token: e, Index: null as int?))
            );
        }

        return tokenIndexes
            .OrderBy(e => e.Token.OriginTokenLocation)
            .OrderBy(e => e.Index)
            .OrderBy(e => e.Token.EngineTokenId);
    }

    public Func<ProjectDbContext, Guid, IEnumerable<string>?, IEnumerable<(Models.Token Token, int? OriginTokenLocationIndex)>> GetTokenizedCorpusTokens = 
        (projectDbContext, tokenizedCorpusId, engineTokenIdAdditions) =>
            {
                return OrganizeTokensByOriginTokenLocation(projectDbContext.Tokens
                    .AsNoTrackingWithIdentityResolution()
                    .Include(e => e.VerseRow)
					.Include(e => e.Grammar)
					.Where(e => e.TokenizedCorpusId == tokenizedCorpusId),
                    engineTokenIdAdditions
                );
            };

    public static GeneralModel<Models.Token> BuildModelSnapshot((Models.Token Token, int? Index) tokenOriginTokenLocationIndex, BuilderContext builderContext)
    {
        var modelProperties = ExtractUsingModelRefs(
            tokenOriginTokenLocationIndex.Token, 
            builderContext, 
            new List<string>() { nameof(Models.IdentifiableEntity.Id), nameof(Models.TokenComponent.VerseRowId), nameof(Models.TokenComponent.GrammarId) });

        modelProperties.Add(VERSE_ROW_LOCATION, (typeof(string), tokenOriginTokenLocationIndex.Token.VerseRow?.BookChapterVerse));
		modelProperties.Add(GRAMMAR_SHORT_NAME, (typeof(string), tokenOriginTokenLocationIndex.Token.Grammar?.ShortName));

		var refValue = CalculateRef(
            tokenOriginTokenLocationIndex.Token.TokenizedCorpusId,
            tokenOriginTokenLocationIndex.Token.EngineTokenId!,
            tokenOriginTokenLocationIndex.Token.OriginTokenLocation,
            tokenOriginTokenLocationIndex.Index
        );

        var tokenModelSnapshot = new GeneralModel<Models.Token>(BuildPropertyRefName(), refValue);
        AddPropertyValuesToGeneralModel(tokenModelSnapshot, modelProperties);

        return tokenModelSnapshot;
    }

    public static string CalculateRef(Guid tokenizedCorpusId, string engineTokenId, string? originTokenLocation, int? index)
    {
        // If a split token, use {OriginTokenLocation}_{Index} (index, when ordered by Subword,
        // within the set of tokens having this OriginTokenLocation).
        // Else use OriginTokenLocation (if there is one - which would mean the EngineTokenId
        // was changed by subword renumbering), otherwise EngineTokenId
        var identityPropertyValue = $"{tokenizedCorpusId}{ originTokenLocation ?? engineTokenId}".ToMD5String();

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
            TokenDeleted = tokenComponent.Deleted is not null
        };
    }

    public static string BuildTokenLocation(Models.Token token)
    {
        return $"{token.BookNumber:000}{token.ChapterNumber:000}{token.VerseNumber:000}{token.WordNumber:000}{token.SubwordNumber:000}";
    }
}
