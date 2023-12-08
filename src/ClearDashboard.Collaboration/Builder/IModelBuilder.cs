using System;
using System.Reflection;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public interface IModelBuilder<T> : IModelBuilder where T: Models.IdentifiableEntity
{
    IEnumerable<GeneralModel<T>> BuildModelSnapshots(BuilderContext builderContext);
    void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings);
}

public interface IModelBuilder
{
    string IdentityKey { get; }
    IEnumerable<PropertyInfo> PropertyInfos { get; }
    IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes { get; }
    IEnumerable<string> NoSerializePropertyNames { get;  }

    GeneralModel BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes);
}