using System;
using System.Collections;

namespace ClearDashboard.Collaboration.DifferenceModel;

public interface IListModelDistinguishable<T> : IListModelDistinguishable
    where T : notnull
{
    /// <summary>
    /// Returns differences between this and an 'other' list
    /// containing instances of the same T.  
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IListDifference<T> GetListDifference(IEnumerable<T> other);
}

public interface IListModelDistinguishable
{
    /// <summary>
    /// Returns differences between this and an 'other' list
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IListDifference GetListDifference(IEnumerable other);
}

public interface IListDifference<out T> : IListDifference
    where T : notnull
{
    /// <summary>
    /// List values only in the first list (based on a comparison
    /// of IModelSnapshot.GetId() values between the two lists.
    /// </summary>
    IEnumerable<T> OnlyIn1 { get; }

    /// <summary>
    /// List values only in the second list (based on a comparison
    /// of IModelSnapshot.GetId() values between the two lists.
    /// </summary>
    IEnumerable<T> OnlyIn2 { get; }

    /// <summary>
    /// IModelDifferences between IModelSnapshot instances found
    /// in both lists (by IModelSnapshot.GetId())
    /// </summary>
    IEnumerable<IModelDifference<T>> ListMemberModelDifferences { get; }
}

public interface IListDifference : IDifference
{
    bool HasMembershipDifferences { get; }
    bool HasMembershipDeletes { get; }
    bool HasMembershipCreates { get; }
    bool HasListMemberModelDifferences { get; }
}
