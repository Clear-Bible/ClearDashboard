using System;
using System.Text.Json.Serialization;

namespace ClearDashboard.Collaboration.DifferenceModel;

public interface IDifference
{
    public bool HasDifferences { get; }
    public bool HasMergeConflict { get; }
}

public static class DifferenceExtensions
{
    public static ModelDifference<T> ToModelDifference<T>(this IEnumerable<PropertyDifference> propertyDifferences, Type modelType, object? id = null)
        where T: notnull 
    {
        var differences = id is null ? new ModelDifference<T>(modelType) : new ModelDifference<T>(modelType, id);
        differences.AddPropertyDifferenceRange(propertyDifferences);
        return differences;
    }

    public static ValueDifference ToValueDifference(this Type propertyType, object? value1, object? value2)
    {
        if (value1 is not null && !value1.GetType().IsAssignableTo(propertyType))
        {
            throw new ArgumentException($"Cannot create ValueDifference - value 1 type {value1.GetType().Name} is not assignable to property type {propertyType.Name}");
        }
        if (value2 is not null && !value2.GetType().IsAssignableTo(propertyType))
        {
            throw new ArgumentException($"Cannot create ValueDifference - value 2 type {value2.GetType().Name} is not assignable to property type {propertyType.Name}");
        }

        Type[] typeArgs = { propertyType };
        Type genericType = typeof(ValueDifference<>).MakeGenericType(typeArgs);
        object?[] constructorArgs = { value1, value2 };

        return (ValueDifference)Activator.CreateInstance(genericType, constructorArgs)!;
    }
}
