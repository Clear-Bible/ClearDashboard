using System;
namespace ClearDashboard.Collaboration.DifferenceModel;

public interface IModelDistinguishable<T> : IModelDistinguishable/*, IEquatable<T> */
    where T : notnull
{
    /// <summary>
    /// Returns differences between this and an 'other' of the same type T
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    IModelDifference<T> GetModelDifference(T other);
}

public interface IModelDistinguishable
{
    /// <summary>
    /// Property values (keyed by property name)
    /// </summary>
    IReadOnlyDictionary<string, object?> PropertyValues { get; }

    /// <summary>
    /// Returns differences between this and an 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    IModelDifference GetModelDifference(object other);

    /// <summary>
    /// Sets ValueType/string properties to 'Value2' difference values,
    /// and if applicable, propagates any IModelDifference(s)
    /// to properties that are model instances.
    /// </summary>
    /// <param name="propertyDifference"></param>
    void ApplyPropertyDifference(PropertyDifference propertyDifference);
}

public interface IModelDifference<out T> : IModelDifference// where T : notnull
{
}

public interface IModelDifference : IDifference
{
    Type ModelType { get; }
    object? Id { get; }
    IEnumerable<PropertyDifference> PropertyDifferences { get; }
    IEnumerable<(string propertyName, IModelDifference propertyModelDifference)> PropertyModelDifferences { get; }
    IEnumerable<(string propertyName, ValueDifference propertyValueDifference)> PropertyValueDifferences { get; }
	IEnumerable<(string propertyName, ListDifference propertyValueDifference)> PropertyListDifferences { get; }
	IReadOnlyDictionary<string, IListDifference> ChildListDifferences { get; }
}

