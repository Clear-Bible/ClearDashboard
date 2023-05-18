using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Model;

public class DomainEntityRef : ModelRef<DomainEntityRef>
{
    public Guid DomainEntityIdGuid { get; set; } = Guid.Empty;
    public string DomainEntityIdName { get; set; } = string.Empty;

    [JsonIgnore]
    public override IReadOnlyDictionary<string, object?> PropertyValues => new Dictionary<string, object?>() {
        { nameof(DomainEntityRef.DomainEntityIdGuid), DomainEntityIdGuid },
        { nameof(DomainEntityRef.DomainEntityIdName), DomainEntityIdName }
    }.AsReadOnly();

    public override IModelDifference<DomainEntityRef> GetModelDifference(DomainEntityRef other)
    {
        var differences = new ModelDifference<DomainEntityRef>(/* This whole class is sort of like an Id, so it doesn't have an Id to put here */this.GetType());

        if (DomainEntityIdGuid != other.DomainEntityIdGuid) { differences.AddPropertyDifference(new PropertyDifference(nameof(DomainEntityIdGuid), new ValueDifference<Guid>(DomainEntityIdGuid, other.DomainEntityIdGuid))); }
        if (DomainEntityIdName != other.DomainEntityIdName) { differences.AddPropertyDifference(new PropertyDifference(nameof(DomainEntityIdName), new ValueDifference<string>(DomainEntityIdName, other.DomainEntityIdName))); }

        return differences;
    }

    public override void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;

        if (nameof(DomainEntityIdGuid) == propertyName) { DomainEntityIdGuid = (Guid)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
        if (nameof(DomainEntityIdName) == propertyName) { DomainEntityIdName = (string)((ValueDifference)propertyDifference.PropertyValueDifference).Value2AsObject!; }
    }

    public override string ToString()
    {
        return DomainEntityIdName + "-" + DomainEntityIdGuid;
    }

    public override bool Equals(object? obj) => Equals(obj as DomainEntityRef);
    public override bool Equals(DomainEntityRef? other)
    {
        if (other == null) return false;
        return this.DomainEntityIdGuid == other.DomainEntityIdGuid && this.DomainEntityIdName == other.DomainEntityIdName;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(DomainEntityIdGuid, DomainEntityIdName);
    }

    public static bool operator ==(DomainEntityRef? e1, DomainEntityRef? e2) => object.Equals(e1, e2);
    public static bool operator !=(DomainEntityRef? e1, DomainEntityRef? e2) => !(e1 == e2);
}
