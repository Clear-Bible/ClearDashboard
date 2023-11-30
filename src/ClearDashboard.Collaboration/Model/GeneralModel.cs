using System.Text.Json.Serialization;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DAL.Alignment.Corpora;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.Collaboration.Exceptions;
using Newtonsoft.Json.Linq;

namespace ClearDashboard.Collaboration.Model;

/// <summary>
/// Wraps the properties of any entity into a general "snapshot" model that allows
/// for common serialization and deserialization routines, equality comparisons,
/// and determination of differences from a snapshot model of the same entity at a
/// different point in time.  Also allows for containing of named "children" lists
/// containing related general "snapshot" models that make sense to serialize/
/// deserialize in sufolders/files, as well as check and apply differences in the
/// context of their parent model.  (E.g. when a specific TokenizedCorpus model
/// differences are being applied to a database, it may make sense to also apply
/// differences of any CompositeTokens contained within the TokenizedCorpus. )
/// </summary>
/// <typeparam name="T"></typeparam>
[JsonConverter(typeof(GeneralModelJsonConverter))]
public class GeneralModel<T> : GeneralModel, IModelSnapshot<T>
    where T : notnull
{
    public GeneralModel(string identityKey, string identityValue, string? comparableId = null) :
        base(identityKey, identityValue, comparableId)
    {
    }

    public GeneralModel(string identityKey, ValueType identityValue, string? comparableId = null) :
        base(identityKey, identityValue, comparableId)
    {
    }

    public GeneralModel(
        string identityKey,
        Dictionary<string, object?> properties,
        Dictionary<string, string>? addedPropertyTypes = null,
        Dictionary<string, IEnumerable<IModelSnapshot>>? children = null) :
        base(identityKey, properties, addedPropertyTypes, children)
    {
    }

    public override IModelDifference GetModelDifference(object other)
    {
        if (other is IModelSnapshot<T>) { return this.GetModelDifference((IModelSnapshot<T>)other); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    public override IModelDifference<IModelSnapshot> GetModelDifference(IModelSnapshot other)
    {
        if (other is IModelSnapshot<T>) { return (IModelDifference<IModelSnapshot>)this.GetModelDifference((IModelSnapshot<T>)other); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }

    public IModelDifference<IModelSnapshot<T>> GetModelDifference(IModelSnapshot<T> other)
    {
        return ((IModelSnapshot<T>)this).FindModelDifference(other);
    }

    protected override void AddProperty(string key, object? value, Type? nullValueValueType = null)
    {
        _properties.Add(key, value);

        // If adding a property whose name does not match one of the
        // wrapped model's properties (and thus the model cannot be
        // used to determine the property type), use reflection to
        // get the type and add it to 'otherProperties' so it gets
        // serialized/deserialized
        if (!typeof(T).GetProperties().Where(p => p.Name == key).Any())
        {
            Type? type = value?.GetType() ?? nullValueValueType;
            if (type is not null)
            {
                if (_addedPropertyTypeNames is null) _addedPropertyTypeNames = new();
                _addedPropertyTypeNames.Add(key, type.ModelSnapshotTypeNameFromType());
            }
        }
    }

    protected override bool TryGetPropertyType(string propertyName, out Type? type)
    {
        type = null;
        if (_properties.ContainsKey(propertyName) &&
            typeof(T).TryGetModelSnapshotPropertyType(_addedPropertyTypeNames, propertyName, out type))
        {
            return true;
        }

        return type is not null;
    }

    public override Type EntityType => typeof(T);

    public override bool Equals(object? other) => Equals(other as GeneralModel<T>);
    public override bool Equals(IModelSnapshot? other) => Equals(other as GeneralModel<T>);
    public bool Equals(IModelSnapshot<T>? other) => Equals(other as GeneralModel<T>);
    public bool Equals(GeneralModel<T>? other)
    {
        if (other == null) return false;
        return this._properties.SequenceEqual(other._properties);
    }
    public override int GetHashCode()
    {
        return this._properties.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()));
    }

    public static bool operator ==(GeneralModel<T>? e1, GeneralModel<T>? e2) => object.Equals(e1, e2);
    public static bool operator !=(GeneralModel<T>? e1, GeneralModel<T>? e2) => !(e1 == e2);
}

public abstract class GeneralModel : IModelSnapshot, IModelDistinguishable<IModelSnapshot>
{
    public GeneralModel(string identityKey, string identityValue, string? comparableId) : this(identityKey, (object)identityValue, comparableId)
    {
    }

    public GeneralModel(string identityKey, ValueType identityValue, string? comparableId) : this(identityKey, (object)identityValue, comparableId)
    {
    }

    public GeneralModel(
        string identityKey,
        Dictionary<string, object?> properties,
        Dictionary<string, string>? addedPropertyTypes = null,
        Dictionary<string, IEnumerable<IModelSnapshot>>? children = null)
    {
        if (!properties.ContainsKey(identityKey) || properties[identityKey] is null)
        {
            throw new ArgumentNullException($"Missing identity value for key {identityKey}");
        }

        if (!(properties[identityKey] is string) && !properties[identityKey]!.GetType().IsValueType)
        {
            throw new ArgumentNullException($"Identity value for key {identityKey} is not a string or ValueType");
        }

        IdentityKey = identityKey;

        if (addedPropertyTypes is null)
        {
            _properties = new();
            foreach (var kvp in properties)
            {
                AddProperty(kvp.Key, kvp.Value);
            }
        }
        else
        {
            _properties = properties;
            _addedPropertyTypeNames = addedPropertyTypes;
        }

        _children = new();
        if (children is not null)
        {
            foreach (var kvp in children)
            {
                AddChild(kvp.Key, kvp.Value);
            }
        }
    }

    private GeneralModel(string identityKey, object identityValue, string? comparableId)
    {
        _properties = new();
        _children = new();
        _comparableId = comparableId;

        IdentityKey = identityKey;
        AddProperty(identityKey, identityValue);
    }
    public string IdentityKey { get; }
    protected Dictionary<string, object?> _properties { get; set; }
    protected Dictionary<string, string>? _addedPropertyTypeNames { get; set; }
    protected Dictionary<string, IEnumerable<IModelDistinguishable>> _children { get; set; } // Not part of equality check, not serialized/deserialized in same container
    protected string? _comparableId { get; }

    // IModelSnapshot:
    public abstract Type EntityType { get; }
    public IReadOnlyDictionary<string, object?> PropertyValues => _properties.AsReadOnly();
    public IReadOnlyDictionary<string, Type> PropertyTypes =>
        _properties.Keys
            .Select(p =>
            {
                if (!TryGetPropertyType(p, out var type))
                    throw new Exception($"Invalid state - no type information for property {p} of model type {EntityType.ShortDisplayName()}");
                return (p, t: type!);
            })
            .ToDictionary(pt => pt.p, pt => pt.t)
            .AsReadOnly();

    public IDictionary<string, (Type type, object? value)> ModelPropertiesTypes =>
        _properties
            .ToDictionary(
                p => p.Key,
                p => 
                {
                    if (TryGetPropertyType(p.Key, out var type))
                        throw new Exception($"Invalid state - no type information for property {p.Key} of model type {EntityType.ShortDisplayName()}");
                    return (type!, p.Value);
                });

    public IReadOnlyDictionary<string, IEnumerable<IModelDistinguishable>> Children => _children.AsReadOnly();
    public IReadOnlyDictionary<string, string>? AddedPropertyTypeNames => _addedPropertyTypeNames?.AsReadOnly();

    // IModelIdentifiable
    public object GetId() => _properties[IdentityKey] ?? throw new Exception($"Unable to determine identity!");
    public string GetComparableId() => _comparableId ?? _properties[IdentityKey]?.ToString() ?? throw new Exception($"Unable to determine identity!");

    // Limit what property types are accepted:
    public void Add(string key, string? value, Type? nullValueValueType = null) { AddProperty(key, value, nullValueValueType); }
    public void Add(string key, ValueType? value, Type? nullValueValueType = null) { AddProperty(key, value, nullValueValueType); }
    public void Add(string key, ModelRef value) { AddProperty(key, value); }
    public void Add(string key, ModelExtra value) { AddProperty(key, value); }
    public void Add(string key, IEnumerable<string> value) { AddProperty(key, value); }
    public void Add(string key, IDictionary<string, object> value) { AddProperty(key, value); }

    public bool TryGetPropertyValue(string key, out object? value) => _properties.TryGetValue(key, out value);
    public object? this[string key] => _properties[key];

    public void AddChild<C>(string key, IEnumerable<IModelDistinguishable<C>> value) where C : notnull => _children.Add(key, value);
    public bool TryGetChildValue(string key, out IEnumerable<IModelDistinguishable>? value) => _children.TryGetValue(key, out value);

    protected abstract void AddProperty(string key, object? value, Type? nullValueValueType = null);
    protected abstract bool TryGetPropertyType(string propertyName, out Type? type);

    public abstract bool Equals(IModelSnapshot? other);
    public abstract IModelDifference GetModelDifference(object other);
    public abstract IModelDifference<IModelSnapshot> GetModelDifference(IModelSnapshot other);

    public override string ToString()
    {
        return GetId().ToString()!;
    }

    public void ApplyPropertyDifference(PropertyDifference propertyDifference)
    {
        var propertyName = propertyDifference.PropertyName;
        if (_properties.TryGetValue(propertyName, out var propertyValue))
        {
            if (propertyValue is not null && propertyValue.GetType().IsAssignableTo(typeof(ModelRef)))
            {
                foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
                {
                    ((ModelRef)propertyValue).ApplyPropertyDifference(pd);
                }
            }
            else if (propertyValue is not null && propertyValue.GetType().IsAssignableTo(typeof(IDictionary<string, object>)))
            {
                foreach (var pd in ((IModelDifference)propertyDifference.PropertyValueDifference).PropertyDifferences)
                {
                    var dictionaryProperty = (IDictionary<string, object>)propertyValue;
                    var value = ((ValueDifference)pd.PropertyValueDifference).Value2AsObject;

                    if (value is null)
                    {
                        dictionaryProperty.Remove(pd.PropertyName);
                    }
                    else
                    {
                        dictionaryProperty[pd.PropertyName] = value;
                    }
                }
            }
            else
            {
                throw new InvalidDifferenceStateException($"Attempt in GeneralModel to ApplyPropertyDifferences to property name '{propertyName}' but value is either null or not of type ModelRef or IEnumerable<KeyValuePair<string, object?>>");
            }
        }
        else
        {
            throw new InvalidDifferenceStateException($"Attempt in GeneralModel to ApplyPropertyDifferences to property name '{propertyName}' but property value not found");
        }
    }
}

// FIXME:  MOVE!
public static class ReflectionExtensions
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type? baseType = givenType?.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }

}