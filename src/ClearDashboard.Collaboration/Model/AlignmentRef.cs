using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Collaboration.DifferenceModel;

namespace ClearDashboard.Collaboration.Model;

public class AlignmentRef : ModelRef<AlignmentRef>
{
    public Guid AlignmentSetId { get; set; } = Guid.Empty;
    public TokenRef SourceTokenRef { get; set; } = new TokenRef();
    public TokenRef TargetTokenRef { get; set; } = new TokenRef();

    [JsonIgnore]
    public override IReadOnlyDictionary<string, object?> PropertyValues => new Dictionary<string, object?>() {
        { nameof(AlignmentRef.AlignmentSetId), AlignmentSetId },
        { nameof(AlignmentRef.SourceTokenRef), SourceTokenRef },
        { nameof(AlignmentRef.TargetTokenRef), TargetTokenRef }
    }.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyDictionary<string, object?> MergeablePropertyValues => PropertyValues;

    public override ModelDifference<AlignmentRef> GetModelDifference(AlignmentRef other)
    {
        var differences = new ModelDifference<AlignmentRef>(/* This whole class is sort of like an Id, so it doesn't have an Id to put here */this.GetType());

        if (AlignmentSetId != other.AlignmentSetId)
        {
            differences.AddPropertyDifference(new PropertyDifference(nameof(AlignmentSetId), new ValueDifference<Guid>(AlignmentSetId, other.AlignmentSetId)));
        }
        if (SourceTokenRef != other.SourceTokenRef)
        {
            differences.AddPropertyDifference(new PropertyDifference(nameof(SourceTokenRef), SourceTokenRef.GetModelDifference(other.SourceTokenRef)));
        }
        if (TargetTokenRef != other.TargetTokenRef)
        {
            differences.AddPropertyDifference(new PropertyDifference(nameof(TargetTokenRef), TargetTokenRef.GetModelDifference(other.TargetTokenRef)));
        }

        return differences;
    }

    public override void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;

        if (nameof(AlignmentSetId) == propertyName) { AlignmentSetId = (Guid)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
        if (nameof(SourceTokenRef) == propertyName)
        {
            foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
            {
                SourceTokenRef.ApplyPropertyDifference(pd);
            }
        }
        if (nameof(TargetTokenRef) == propertyName)
        {
            foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
            {
                TargetTokenRef.ApplyPropertyDifference(pd);
            }
        }
    }

    public override string ToString()
    {
        return AlignmentSetId + "-" + SourceTokenRef.ToString() + "-" + TargetTokenRef.ToString();
    }

    public override bool Equals(object? obj) => Equals(obj as AlignmentRef);
    public override bool Equals(AlignmentRef? other)
    {
        if (other == null) return false;
        return
            this.AlignmentSetId == other.AlignmentSetId &&
            this.SourceTokenRef == other.SourceTokenRef &&
            this.TargetTokenRef == other.TargetTokenRef;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(AlignmentSetId, SourceTokenRef.GetHashCode(), TargetTokenRef.GetHashCode());
    }
    public static bool operator ==(AlignmentRef? e1, AlignmentRef? e2) => object.Equals(e1, e2);
    public static bool operator !=(AlignmentRef? e1, AlignmentRef? e2) => !(e1 == e2);
}
