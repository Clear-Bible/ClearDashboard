using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Model;

public class TranslationRef : ModelRef<TranslationRef>
{
    public Guid TranslationSetId { get; set; } = Guid.Empty;
    public TokenRef SourceTokenRef { get; set; } = new TokenRef();

    [JsonIgnore]
    public override IReadOnlyDictionary<string, object?> PropertyValues => new Dictionary<string, object?>() {
        { nameof(TranslationRef.TranslationSetId), TranslationSetId },
        { nameof(TranslationRef.SourceTokenRef), SourceTokenRef }
    }.AsReadOnly();

    public override IModelDifference<TranslationRef> GetModelDifference(TranslationRef other)
    {
        var differences = new ModelDifference<TranslationRef>(/* This whole class is sort of like an Id, so it doesn't have an Id to put here */this.GetType());

        if (TranslationSetId != other.TranslationSetId)
        {
            differences.AddPropertyDifference(new PropertyDifference(nameof(TranslationSetId), new ValueDifference<Guid>(TranslationSetId, other.TranslationSetId)));
        }
        if (SourceTokenRef != other.SourceTokenRef)
        {
            differences.AddPropertyDifference(new PropertyDifference(nameof(SourceTokenRef), SourceTokenRef.GetModelDifference(other.SourceTokenRef)));
        }

        return differences;
    }

    public override void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;

        if (nameof(TranslationSetId) == propertyName) { TranslationSetId = (Guid)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
        if (nameof(SourceTokenRef) == propertyName)
        {
            foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
            {
                SourceTokenRef.ApplyPropertyDifference(pd);
            }
        }
    }

    public override string ToString()
    {
        return TranslationSetId + "-" + SourceTokenRef.ToString();
    }

    public override bool Equals(object? obj) => Equals(obj as TranslationRef);
    public override bool Equals(TranslationRef? other)
    {
        if (other == null) return false;
        return this.TranslationSetId == other.TranslationSetId && this.SourceTokenRef == other.SourceTokenRef;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(TranslationSetId, SourceTokenRef.GetHashCode());
    }
    public static bool operator ==(TranslationRef? e1, TranslationRef? e2) => object.Equals(e1, e2);
    public static bool operator !=(TranslationRef? e1, TranslationRef? e2) => !(e1 == e2);
}
