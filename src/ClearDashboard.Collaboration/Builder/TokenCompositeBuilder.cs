using Microsoft.EntityFrameworkCore;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data;
using System.Text.Json;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using System.Text;

namespace ClearDashboard.Collaboration.Builder;

public class TokenCompositeBuilder : GeneralModelBuilder<Models.TokenComposite>
{
    public const string VERSE_ROW_LOCATION = "VerseRowLocation";
    public const string TOKEN_LOCATIONS = "TokenLocations";
    public override string IdentityKey => BuildPropertyRefName();

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

        var modelProperties = ExtractUsingModelRefs(
            tokenComposite,
            builderContext,
            new List<string>() { "Id", "VerseRowId" });

        modelSnapshot.Add(VERSE_ROW_LOCATION, tokenComposite.VerseRow?.BookChapterVerse, typeof(string));
        modelSnapshot.Add(TOKEN_LOCATIONS, childTokens.Select(t => TokenBuilder.BuildTokenLocation(t)).ToGeneralListModel<string>());

        var refValue = CalculateRef(
            tokenComposite.TokenizedCorpusId,
            tokenComposite.ParallelCorpusId,
            tokenComposite.EngineTokenId!
        );

        var tokenCompositeModelSnapshot = new GeneralModel<Models.TokenComposite>(BuildPropertyRefName(), refValue);
        AddPropertyValuesToGeneralModel(tokenCompositeModelSnapshot, modelProperties);

        return tokenCompositeModelSnapshot;
    }

    private static string CalculateRef(Guid tokenizedCorpusId, Guid? parallelCorpusId, string engineTokenId)
    {
        var sb = new StringBuilder();

        sb.Append(tokenizedCorpusId.ToString());
        if (parallelCorpusId != null)
        {
            sb.Append(parallelCorpusId);
        }
        sb.Append(engineTokenId);

        var identityPropertyValue = sb.ToString().ToMD5String();
        return $"TokenComposite_{identityPropertyValue}";
    }

    public override GeneralModel<TokenComposite> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        // If the TokenComposite model properties are the older style - where there is an "Id" instead
        // of a "Ref", convert the Id to a Ref before building the snapshot:
        if (modelPropertiesTypes.TryGetValue(nameof(Models.TokenComposite.Id), out var idPropertyValue))
        {
            if (modelPropertiesTypes.TryGetValue(nameof(Models.TokenComposite.TokenizedCorpusId), out var tokenizedCorpusValue) &&
                modelPropertiesTypes.TryGetValue(nameof(Models.TokenComposite.EngineTokenId), out var engineTokenIdValue))
            {
                modelPropertiesTypes.TryGetValue(nameof(Models.TokenComposite.ParallelCorpusId), out var parallelCorpusIdValue);

                var refId = CalculateRef((Guid)tokenizedCorpusValue.value!, (Guid?)parallelCorpusIdValue.value, (string)engineTokenIdValue.value!);

                modelPropertiesTypes.Remove(nameof(Models.TokenComposite.Id));
                modelPropertiesTypes.Add(IdentityKey, (type: typeof(string), value: refId));
            }
        }

        return base.BuildGeneralModel(modelPropertiesTypes);
    }
}
