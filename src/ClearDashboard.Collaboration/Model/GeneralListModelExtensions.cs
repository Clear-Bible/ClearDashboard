using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClearDashboard.Collaboration.Model;

public static class GeneralListModelExtensions
{
    public static GeneralListModel<TSource> ToGeneralListModel<TSource>(this IEnumerable<TSource> source)
        where TSource : notnull
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return new GeneralListModel<TSource>(source);
    }

    public static IEnumerable<IModelDistinguishable<IModelSnapshot<TSource>>> AsModelSnapshotChildrenList<TSource>(this IEnumerable<IModelSnapshot<TSource>> source)
        where TSource : notnull
    {
        if (!source.GetType().IsAssignableToGenericType(typeof(GeneralListModel<>)))
        {
            source = source.ToGeneralListModel();
        }
        return source.Cast<IModelDistinguishable<IModelSnapshot<TSource>>>();
    }

    // In Child list:  IEnumerable<IModelDistinguishable<IModelSnapshot<Models.Alignment>>>
    // Incoming:       IEnumerable<GeneralModel<Models.Alignment>>
    public static IListDifference<TSource> GetListDifference<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        where TSource : notnull // Ideally this would be IEquatable<T>, but we need to call this where TSource is non-generic
    {
        var listMembershipDifference = source.GetListMembershipDifference(other);
        var listMemberModelDifferences = Enumerable.Empty<IModelDifference<TSource>>();

        // We can only do the model comparison if we know the TSource model type:
        if (typeof(TSource).IsAssignableTo(typeof(IModelDistinguishable<TSource>)))
        {
            listMemberModelDifferences = source.GetListMemberModelDifferences(other);
        }
        else if (!typeof(TSource).IsValueType && !(typeof(TSource) == typeof(string)))
        {
            // This is ok if TSource is something like 'string', otherwise let's
            // throw an exception:
            throw new InvalidModelStateException($"Model exception:  unable to GetListMemberModelDifferences for list item type '{typeof(TSource).ShortDisplayName()}' since it does not implmement 'IModelDistinguishable<TSource>'");
        }

        return new ListDifference<TSource>(listMembershipDifference, listMemberModelDifferences);
    }

    public static ListMembershipDifference<TSource> GetListMembershipDifference<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        where TSource : notnull
    {
        if (typeof(TSource).IsAssignableTo(typeof(IModelSnapshot)))
        {
            var onlyInThis = source
                .ExceptBy(
                    other
                        .Where(i => i != null)
                        .Select(i => ((IModelSnapshot)i!).GetId()),
                    i => ((IModelSnapshot)i!).GetId())
                .ToList();

            var onlyInOther = other
                .ExceptBy(
                    source
                        .Where(i => i != null)
                        .Select(i => ((IModelSnapshot)i!).GetId()),
                    i => ((IModelSnapshot)i!).GetId())
                .ToList();

            return new ListMembershipDifference<TSource>(onlyInThis, onlyInOther);
        }
        else if (typeof(TSource).IsAssignableTo(typeof(IEquatable<TSource>)))
        {
            return new ListMembershipDifference<TSource>(source.Except(other).ToList(), other.Except(source).ToList());
        }
        else
        {
            throw new ArgumentException($"Unable to find ListMembershipDifference for type {typeof(TSource).ShortDisplayName()}");
        }
    }

    public static IEnumerable<IModelDifference<TSource>> GetListMemberModelDifferences<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        where TSource : notnull
    {
        if (typeof(TSource).IsAssignableTo(typeof(IModelDistinguishable<TSource>)))
        {
            var memberModelDifferences = new List<IModelDifference<TSource>>();

            var inBothThis = source
                .IntersectBy(other
                    .Select(i => ((IModelSnapshot)i!).GetId()), i => ((IModelSnapshot)i!).GetId())
                .ToDictionary(i => ((IModelSnapshot)i!).GetId(), i => (IModelDistinguishable<TSource>)i);

            var inBothOther = other
                .IntersectBy(source
                    .Select(i => ((IModelSnapshot)i!).GetId()), i => ((IModelSnapshot)i!).GetId())
                .ToDictionary(i => ((IModelSnapshot)i!).GetId(), i => (IModelDistinguishable<TSource>)i);

            foreach (var id in inBothThis.Keys)
            {
                //if (!((TSource)inBothThis[id]).Equals((TSource)inBothOther[id]))
                //{
                var memberModelDifference = inBothThis[id].GetModelDifference((TSource)inBothOther[id]);
                if (memberModelDifference is not null && memberModelDifference.HasDifferences)
                {
                    memberModelDifferences.Add(memberModelDifference);
                }
                //}
            }

            return memberModelDifferences;
        }
        else
        {
            throw new ArgumentException($"GeneralListModel<{typeof(TSource).ShortDisplayName()}> generic type parameter does not implement IModelDistinguishable<T>");
        }
    }

    public static T? FindById<T>(this IEnumerable<T>? modelSnapshots, object id)
        where T : IModelSnapshot
    {
        return (modelSnapshots is not null)
            ? modelSnapshots!.Where(e => e.GetId().Equals(id)).FirstOrDefault()
            : default(T);
    }
}
