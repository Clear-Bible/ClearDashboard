using System;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using ClearDashboard.Collaboration.Model;

namespace ClearDashboard.Collaboration.DifferenceModel;

public class ModelDifference<T> : ModelDifference, IModelDifference<T>// where T : notnull
{
    public ModelDifference(Type modelType) : base(modelType)
    {
    }

    public ModelDifference(Type modelType, object id) : base(modelType, id)
    {
    }
}

public abstract class ModelDifference : IModelDifference
{
    private readonly List<PropertyDifference> _propertyDifferences;           // Each PropertyDifference has a ValueDifference 
    private readonly Dictionary<string, IListDifference> _childListDifferences;

    public ModelDifference(Type modelType)
    {
        _propertyDifferences = new List<PropertyDifference>();
        _childListDifferences = new Dictionary<string, IListDifference>();
        ModelType = modelType;
    }

    public ModelDifference(Type modelType, object id)
    {
        _propertyDifferences = new List<PropertyDifference>();
        _childListDifferences = new Dictionary<string, IListDifference>();
        ModelType = modelType;
        Id = id;
    }

    public Type ModelType { get; private set; }
    public object? Id { get; private set; }

    public IEnumerable<PropertyDifference> PropertyDifferences { get => _propertyDifferences; }

    [JsonIgnore]
    public IEnumerable<(string propertyName, IModelDifference propertyModelDifference)> PropertyModelDifferences => _propertyDifferences
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(IModelDifference)))
            .Select(pd => (pd.PropertyName, (IModelDifference)pd.PropertyValueDifference));

    [JsonIgnore]
    public IEnumerable<(string propertyName, ValueDifference propertyValueDifference)> PropertyValueDifferences => _propertyDifferences
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ValueDifference)))
            .Select(pd => (pd.PropertyName, (ValueDifference)pd.PropertyValueDifference));

	[JsonIgnore]
	public IEnumerable<(string propertyName, ListDifference propertyValueDifference)> PropertyListDifferences => _propertyDifferences
			.Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ListDifference)))
			.Select(pd => (pd.PropertyName, (ListDifference)pd.PropertyValueDifference));

	public IReadOnlyDictionary<string, IListDifference> ChildListDifferences { get => _childListDifferences; }
    public void AddPropertyDifference(PropertyDifference propertyDifference) { _propertyDifferences.Add(propertyDifference); }
    public void AddPropertyDifferenceRange(IEnumerable<PropertyDifference> propertyDifferences) { _propertyDifferences.AddRange(propertyDifferences); }
    public void AddChildListDifference(string childName, IListDifference listDifference)
    {
        _childListDifferences.Add(childName, listDifference);
    }

    [JsonIgnore]
    public bool HasDifferences { get => _propertyDifferences.Any() || _childListDifferences.Any(); }

    [JsonIgnore]
    public bool HasMergeConflict => PropertyDifferences.Where(e => e.HasMergeConflict).Any();

    public bool TryGetChildListDifference(string childName, out IListDifference? listDifference)
    {
        listDifference = null;
        if (_childListDifferences.ContainsKey(childName))
        {
            listDifference = _childListDifferences[childName];
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        string modelTypeAsString = ModelType.ShortDisplayName();

        if (ModelType.IsAssignableToGenericType(typeof(GeneralModel<>)))
        {
            modelTypeAsString = ModelType.GetGenericArguments()[0].ShortDisplayName();
        }

        if (Id is not null)
        {
            return $"ModelDifference for {modelTypeAsString}[{Id}] (count: {PropertyDifferences.Count()})";
        }
        else
        {
            return $"ModelDifference for {modelTypeAsString} (count: {PropertyDifferences.Count()})";
        }
    }
}

public enum ModelMergeResult
{
    ShouldMerge,
    NoChange,
    Conflict,
    Unset
}