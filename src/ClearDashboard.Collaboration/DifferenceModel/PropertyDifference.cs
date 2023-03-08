using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClearDashboard.Collaboration.DifferenceModel;

public class PropertyDifference : IDifference
{
    public PropertyDifference(string propertyName, IDifference propertyValueDifference)
    {
        PropertyName = propertyName;
        PropertyValueDifference = propertyValueDifference;
    }

    public string PropertyName { get; private set; }
    public IDifference PropertyValueDifference { get; private set; }

    public bool HasDifferences => true;
    public bool HasMergeConflict => PropertyValueDifference.HasMergeConflict;

    public override string ToString()
    {
        return $"{PropertyName}:" + PropertyValueDifference.ToString();
    }
}

public class ValueDifference<T> : ValueDifference
{
    public ValueDifference(T? value1, T? value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    public T? Value1 { get; private set; }
    public T? Value2 { get; private set; }

    public override object? Value1AsObject => Value1;
    public override object? Value2AsObject => Value2;

    public override bool EqualsValue1(object? compareValue1)
    {
        if (object.ReferenceEquals(Value1, compareValue1)) { return true; }
        if (compareValue1 is null) { return false; }
        if (compareValue1.GetType() != typeof(T)) { return false; }

        return EqualityComparer<T>.Default.Equals(Value1, (T?)compareValue1);
    }

    public override bool EqualsValue2(object? compareValue2)
    {
        if (object.ReferenceEquals(Value2, compareValue2)) { return true; }
        if (compareValue2 is null) { return false; }
        if (compareValue2.GetType() != typeof(T)) { return false; }

        return EqualityComparer<T>.Default.Equals(Value2, (T?)compareValue2);
    }

    public override string ToString()
    {
        return $"Value 1: '{Value1}', Value 2: '{Value2}'";
    }
}

public abstract class ValueDifference : IDifference
{
    // Otherwise this instance wouldn't have been created
    public bool HasDifferences => true;
    public bool HasMergeConflict => (ConflictValue is not null);

    public abstract bool EqualsValue1(object? compareValue1);
    public abstract bool EqualsValue2(object? compareValue1);

    public abstract object? Value1AsObject { get; }
    public abstract object? Value2AsObject { get; }

    public object? ConflictValue { get; set; }
}