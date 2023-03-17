﻿using ClearDashboard.Collaboration.DifferenceModel;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public interface IModelSnapshot<T> : IModelDistinguishable<IModelSnapshot<T>>, IModelSnapshot where T : notnull
{
}

public interface IModelSnapshot : IModelDistinguishable<IModelSnapshot> // IModelIdentifiable // OR IModelExternalizable?
{
    /// <summary>
    /// Id of the entity represented by this IModelSnapshot
    /// </summary>
    /// <returns></returns>
    object GetId();

    /// <summary>
    /// Type of Entity Framework entity this IModelSnapshot represents
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Name of property (from PropertyValues) from which the IModelSnapshot's
    /// "Id" can be obtained.  Primarily used in serialization/deserialization
    /// if IModelSnapshot (@see IModelSnapshotJsonConverter)
    /// </summary>
    string IdentityKey { get; }

    // FIXME:  This might not be needed anymore since in the Merge handlers
    // we are supplying property mappings and resolvers to get real entity
    // properties/values
    /// <summary>
    /// Property values (keyed by property name)
    /// </summary>
    IReadOnlyDictionary<string, object?> EntityPropertyValues => PropertyValues;

    /// <summary>
    /// Children of this IModelSnapshot, meaning not properties of the entity
    /// this is representing, but other related entity groups.
    /// </summary>
    IReadOnlyDictionary<string, IEnumerable<IModelDistinguishable>> Children { get; }

    /// <summary>
    /// Types of all property values by property value name (includes
    /// any 'additional' properties.  Needed when determining property
    /// differences between two IModelSnapshots.
    /// </summary>
    IReadOnlyDictionary<string, Type> PropertyTypes { get; }

    /// <summary>
    /// Property Type names not part of the entity this IModelSnapshot is
    /// representing, that are handled in a specific way during
    /// serialization / deserialization.
    /// </summary>
    IReadOnlyDictionary<string, string>? AddedPropertyTypeNames { get; }

    bool TryGetPropertyValue(string key, out object? value);

}

//public interface IModelIdentifiable : IModelDistinguishable<IModelIdentifiable>
//{
//    object GetId();
//}


// IModelDistinguishable
//  PropertyValues
//
// IModelDistinguishable<T> : IModelDistinguishable  (TokenRef, TranslationRef, etc.)
//  GetDifferences(T other)
//
// IModelIdentifiable: IModelDistinguishable
//  GetId()
//  (So anything that implements IModelIdentifiable has 'PropertyValues', 'GetDifferences(IModelIdentifiable)'
//
// 