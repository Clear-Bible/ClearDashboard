using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Model;

public class TokenRef : ModelRef<TokenRef>
{
    public bool IsComposite { get; set; } = false;
    public Guid TokenizedCorpusId { get; set; } = Guid.Empty;
    public string TokenLocation { get; set; } = string.Empty;
    public string TokenSurfaceText { get; set; } = string.Empty;

    [JsonIgnore]
    public override IReadOnlyDictionary<string, object?> PropertyValues => new Dictionary<string, object?>() {
        { nameof(TokenRef.IsComposite), IsComposite },
        { nameof(TokenRef.TokenizedCorpusId), TokenizedCorpusId },
        { nameof(TokenRef.TokenLocation), TokenLocation },
        { nameof(TokenRef.TokenSurfaceText), TokenSurfaceText }
    }.AsReadOnly();

    public override IModelDifference<TokenRef> GetModelDifference(TokenRef other)
    {
        var differences = new ModelDifference<TokenRef>(/* This whole class is sort of like an Id, so it doesn't have an Id to put here */this.GetType());

        if (IsComposite != other.IsComposite) { differences.AddPropertyDifference(new PropertyDifference(nameof(IsComposite), new ValueDifference<bool>(IsComposite, other.IsComposite))); }
        if (TokenizedCorpusId != other.TokenizedCorpusId) { differences.AddPropertyDifference(new PropertyDifference(nameof(TokenizedCorpusId), new ValueDifference<Guid>(TokenizedCorpusId, other.TokenizedCorpusId))); }
        if (TokenLocation != other.TokenLocation) { differences.AddPropertyDifference(new PropertyDifference(nameof(TokenLocation), new ValueDifference<string>(TokenLocation, other.TokenLocation))); }

        return differences;
    }

    public override void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;
        var valueToApply = ((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!;

        if (nameof(IsComposite) == propertyName) { IsComposite = (bool)valueToApply; }
        if (nameof(TokenizedCorpusId) == propertyName) { TokenizedCorpusId = (Guid)valueToApply; }
        if (nameof(TokenLocation) == propertyName) { TokenLocation = (string)valueToApply; }
    }

    public override string ToString()
    {
        return TokenizedCorpusId + "-" + TokenLocation;
    }

    public override bool Equals(object? obj) => Equals(obj as TokenRef);
    public override bool Equals(TokenRef? other)
    {
        if (other == null) return false;
        return this.TokenizedCorpusId == other.TokenizedCorpusId && this.TokenLocation == other.TokenLocation;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(TokenizedCorpusId, TokenLocation);
    }

    public static bool operator ==(TokenRef? e1, TokenRef? e2) => object.Equals(e1, e2);
    public static bool operator !=(TokenRef? e1, TokenRef? e2) => !(e1 == e2);
}
