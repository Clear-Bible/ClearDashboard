using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.DifferenceModel;

namespace ClearDashboard.Collaboration.Model;

public abstract class ModelRef<T> : ModelRef, IEquatable<T>, IModelDistinguishable<T> where T : notnull
{
    public abstract bool Equals(T? other);

    public abstract IModelDifference<T> GetModelDifference(T other);
    public override IModelDifference GetModelDifference(object other)
    {
        if (other is T) { return this.GetModelDifference((T)other); }
        throw new Exception($"Invalid model comparison between type {this.GetType().Name} and {other.GetType().Name}");
    }
}

public abstract class ModelRef : IModelDistinguishable
{
    public abstract IReadOnlyDictionary<string, object?> PropertyValues { get; }

    public abstract IModelDifference GetModelDifference(object other);
    public abstract void ApplyPropertyDifference(PropertyDifference propertyDifference);
}
