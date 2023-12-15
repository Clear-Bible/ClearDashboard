using System;
using System.Collections;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClearDashboard.Collaboration.DifferenceModel;

public class ListDifference<T> : ListDifference, IListDifference<T>
    where T : notnull
{
    private readonly ListMembershipDifference<T> _listMembershipDifference;
    private readonly List<IModelDifference<T>> _listMemberModelDifferences;

    public ListDifference(ListMembershipDifference<T> listMembershipDifference, IEnumerable<IModelDifference<T>> listMemberModelDifferences)
    {
        _listMembershipDifference = listMembershipDifference;
        _listMemberModelDifferences = new List<IModelDifference<T>>(listMemberModelDifferences);
    }

    public ListMembershipDifference<T> ListMembershipDifference { get => _listMembershipDifference; }
    public IEnumerable<IModelDifference<T>> ListMemberModelDifferences { get => _listMemberModelDifferences; }

    public void AddListMemberModelDifference(IModelDifference<T> listMemberModelDifference) { _listMemberModelDifferences.Add(listMemberModelDifference); }

    public IEnumerable<T> OnlyIn1 => ListMembershipDifference.OnlyIn1;
    public IEnumerable<T> OnlyIn2 => ListMembershipDifference.OnlyIn2;

    [JsonIgnore]
    public override bool HasMembershipDifferences
    {
        get => ListMembershipDifference.OnlyIn1.Any() || ListMembershipDifference.OnlyIn2.Any();
    }

    [JsonIgnore]
    public override bool HasMembershipDeletes
    {
        get => ListMembershipDifference.OnlyIn1.Any();
    }

    [JsonIgnore]
    public override bool HasMembershipCreates
    {
        get => ListMembershipDifference.OnlyIn2.Any();
    }

    [JsonIgnore]
    public override bool HasListMemberModelDifferences
    {
        get => ListMemberModelDifferences.Any();
    }

    [JsonIgnore]
    public override bool HasMergeConflict
    {
        get => ListMemberModelDifferences.Where(e => e.HasMergeConflict).Any();
    }

    public override string ToString()
    {
        return $"ListDifference for type {typeof(T).ShortDisplayName()} (has membership " +
               $"differences: {HasMembershipDifferences}, has member model differences: " +
               $"{ListMemberModelDifferences.Any()})";
    }
}

public abstract class ListDifference : IListDifference, IDifference
{
    [JsonIgnore]
    public abstract bool HasMembershipDifferences { get; }
    [JsonIgnore]
    public abstract bool HasMembershipDeletes { get; }
    [JsonIgnore]
    public abstract bool HasMembershipCreates { get; }
    [JsonIgnore]
    public abstract bool HasListMemberModelDifferences { get; }
    [JsonIgnore]
    public bool HasDifferences => HasMembershipDifferences || HasListMemberModelDifferences;
    [JsonIgnore]
    public abstract bool HasMergeConflict { get; }
}

public class ListMembershipDifference<T> : IDifference where T : notnull
{
    public ListMembershipDifference(IEnumerable<T> onlyIn1, IEnumerable<T> onlyIn2)
    {
        OnlyIn1 = onlyIn1;
        OnlyIn2 = onlyIn2;
    }

    public IEnumerable<T> OnlyIn1 { get; private set; }
    public IEnumerable<T> OnlyIn2 { get; private set; }

    public bool HasDifferences => OnlyIn1.Any() || OnlyIn2.Any();

    public bool HasMergeConflict => false;

    public override string ToString()
    {
        return $"Only in 1: '{string.Join(", ", OnlyIn1)}', Only in 2: '{string.Join(", ", OnlyIn2)}'";
    }
}
