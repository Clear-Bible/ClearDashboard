using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Model
{
    internal static class IModelSnapshotDifferenceExtensions
    {
        internal static IModelDifference<IModelSnapshot<T>> FindModelDifference<T>(this IModelSnapshot<T> modelSnapshot, IModelSnapshot<T> other)
            where T : notnull
        {
            if (!modelSnapshot.GetId().Equals(other.GetId()))
            {
                throw new Exception($"Invalid comparison between {modelSnapshot.GetType().ShortDisplayName()} instances having different Ids '{modelSnapshot.GetId()}' vs '{other.GetId()}'");
            }

            var modelDifference = new ModelDifference<IModelSnapshot<T>>(typeof(T), modelSnapshot.GetId());

            modelDifference.AddPropertyDifferenceRange(modelSnapshot.FindPropertyDifferences(other));

            foreach (var key in (modelSnapshot.Children?.Keys ?? Enumerable.Empty<string>())
                .Union(other.Children?.Keys ?? Enumerable.Empty<string>()))
            {
                var childListDifference = modelSnapshot.FindChildListDifference(key, other);
                if (childListDifference is not null && childListDifference.HasDifferences)
                {
                    modelDifference.AddChildListDifference(key, childListDifference);
                }
            }

            return modelDifference;
        }

        internal static IEnumerable<PropertyDifference> FindPropertyDifferences<T>(this IModelSnapshot<T> modelSnapshot, IModelSnapshot<T> other)
            where T : notnull
        {
            var propertyDifferences = new List<PropertyDifference>();

            var thisPropertyValues = modelSnapshot.PropertyValues;
            var otherPropertyValues = other.PropertyValues;

            var propertyTypes = modelSnapshot.PropertyTypes.Union(other.PropertyTypes)
                .GroupBy(g => g.Key)
                .ToDictionary(pair => pair.Key, pair => pair.First().Value); ;

            foreach (var key in thisPropertyValues.Keys.Union(otherPropertyValues.Keys))
            {
                if (!propertyTypes.TryGetValue(key, out var propertyType) || propertyType is null)
                {
                    throw new InvalidModelStateException($"IModelSnapshot type {typeof(T).Name} has property name {key} for which a property type cannot be determined");
                }

                if (propertyType.IsAssignableTo(typeof(ModelExtra)))
                {
                    // Ignore
                    continue;
                }

                thisPropertyValues.TryGetValue(key, out object? value);
                otherPropertyValues.TryGetValue(key, out object? valueOther);

                var propertyDifference = CheckGetPropertyDifference(value, valueOther, key, propertyType);
                if (propertyDifference is not null)
                {
                    propertyDifferences.Add(propertyDifference);
                }
            }

            return propertyDifferences;
        }

        private static PropertyDifference? CheckGetPropertyDifference(object? value, object? valueOther, string propertyName, Type propertyType)
        {
            if (object.ReferenceEquals(value, valueOther)) return null;
            if (value is null || valueOther is null)
            {
                return new PropertyDifference(propertyName, propertyType.ToValueDifference(value, valueOther));
            }

            PropertyDifference? propertyDifference = null;

            if (!value.Equals(valueOther))
            {
                if (propertyType.IsAssignableTo(typeof(IModelDistinguishable)))
                {
                    // E.g.:  TokenRef, TranslationRef, AlignmentRef
                    var diff = ((IModelDistinguishable)value).GetModelDifference(valueOther);
                    if (diff is not null && diff.HasDifferences)
                    {
                        propertyDifference = new PropertyDifference(propertyName, diff);
                    }
                }
                else if (propertyType.IsAssignableTo(typeof(IEnumerable<string>)))
                {
                    var diff = ((IEnumerable<string>)value).GetListDifference((IEnumerable<string>)valueOther);
                    if (diff.HasDifferences)
                    {
                        propertyDifference = new PropertyDifference(propertyName, diff);
                    }
                }
                else if (propertyType.IsAssignableTo(typeof(IDictionary<string, object>)))
                {
                    var diff = ((IDictionary<string, object>)value).FindMetadataModelDifference((IDictionary<string, object>)valueOther);
                    if (diff.HasDifferences)
                    {
                        propertyDifference = new PropertyDifference(propertyName, diff);
                    }
                }
                else if (propertyType.IsValueType || propertyType == typeof(string))
                {
                    propertyDifference = new PropertyDifference(propertyName, propertyType.ToValueDifference(value, valueOther));
                }
                else
                {
                    throw new NotSupportedException($"Unable to find differences for property type {propertyType.ShortDisplayName()}");
                }
            }

            return propertyDifference;
        }

        /// <summary>
        /// Finds differences in a dictionary (e.g. Metadata entity property), treating
        /// each key / value pair as though it is a model object property name / value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        internal static IModelDifference<IDictionary<string, object>> FindMetadataModelDifference(
            this IDictionary<string, object> source,
            IDictionary<string, object> other)
        {
            var difference = new ModelDifference<IDictionary<string, object>>(typeof(IDictionary<string, object>));

            source.ExceptBy(other.Select(e => e.Key), e => e.Key).ToList().ForEach(e =>
            {
                if (e.Value is not null)
                {
                    difference.AddPropertyDifference(new PropertyDifference(
                        e.Key.ToString()!,
                        new ValueDifference<object>(e.Value, null)));
                }
            });

            other.ExceptBy(source.Select(e => e.Key), e => e.Key).ToList().ForEach(e =>
            {
                if (e.Value is not null)
                {
                    difference.AddPropertyDifference(new PropertyDifference(
                        e.Key.ToString()!,
                        new ValueDifference<object>(null, e.Value)));
                }
            });

            source.IntersectBy(other.Select(e => e.Key), e => e.Key).ToList().ForEach(e =>
            {
                if (object.ReferenceEquals(e.Value, other[e.Key]))
                    return;
                if (e.Value is null || other[e.Key] is null ||
                    e.Value.GetType() != other[e.Key].GetType())
                {
                    difference.AddPropertyDifference(new PropertyDifference(
                        e.Key.ToString()!,
                        new ValueDifference<object>(e.Value, other[e.Key])));
                    return;
                }
                if (e.Value is JsonElement)
                {
                    var element = (JsonElement)e.Value;
                    var otherElement = (JsonElement)other[e.Key];

                    var valueToCompare = ((JsonElement)e.Value).GetRawText();
                    var otherValueToCompare = ((JsonElement)other[e.Key]).GetRawText();

                    if (element.ValueKind != otherElement.ValueKind ||
                        element.GetRawText() != otherElement.GetRawText())
                    {
                        difference.AddPropertyDifference(new PropertyDifference(
                            e.Key.ToString()!,
                            new ValueDifference<object>(e.Value, other[e.Key])));
                    }
                    return;
                }
                if (!e.Value.Equals(other[e.Key]))
                {
                    difference.AddPropertyDifference(new PropertyDifference(
                        e.Key.ToString()!,
                        new ValueDifference<object>(e.Value, other[e.Key])));
                }
            });

            return difference;
        }

        internal static ListDifference? FindChildListDifference<T>(this IModelSnapshot<T> modelSnapshot, string childName, IModelSnapshot<T> otherModelSnapshot)
            where T : notnull
        {
            IEnumerable<IModelDistinguishable>? childList1 = null;
            IEnumerable<IModelDistinguishable>? childList2 = null;
            if (modelSnapshot.Children is null || !modelSnapshot.Children.TryGetValue(childName, out childList1))
            {
                childList1 = Enumerable.Empty<IModelDistinguishable>();
            }
            if (otherModelSnapshot.Children is null || !otherModelSnapshot.Children.TryGetValue(childName, out childList2))
            {
                childList2 = Enumerable.Empty<IModelDistinguishable>();
            }

            if (!childList1.Any() && !childList2.Any())
            {
                return null;
            }

            // From whichever one isn't empty:
            var enumerableGenericArgumentType = childList1.Any()
                ? childList1.GetType().GetGenericArguments()[0]
                : childList2.GetType().GetGenericArguments()[0];

            Type? iModelSnapshotType = null;
            if (enumerableGenericArgumentType.IsAssignableToGenericType(typeof(GeneralModel<>)))
            {
                var iModelSnapshotUnbound = typeof(IModelSnapshot<>);
                Type[] typeArgs = { enumerableGenericArgumentType.GetGenericArguments()[0] };
                iModelSnapshotType = iModelSnapshotUnbound.MakeGenericType(typeArgs);
            }

            if (!childList1.Any() && iModelSnapshotType is not null)
            {
                var toEmptyModelSnapshotTypeMethod = typeof(Enumerable).GetMethod("Empty")!.MakeGenericMethod(iModelSnapshotType);
                childList1 = (IEnumerable<IModelDistinguishable>)toEmptyModelSnapshotTypeMethod.Invoke(null, null)!;
            }

            if (!childList2.Any() && iModelSnapshotType is not null)
            {
                var toEmptyModelSnapshotTypeMethod = typeof(Enumerable).GetMethod("Empty")!.MakeGenericMethod(iModelSnapshotType);
                childList2 = (IEnumerable<IModelDistinguishable>)toEmptyModelSnapshotTypeMethod.Invoke(null, null)!;
            }

            // FIXME:  this fixes lame casting problem between GeneralListModel<GeneralModel<X>>
            // and the GeneralListModelExtensions.GetListDifference (GeneralModel<X> isn't assignableTo
            // IModelDistinguishable<GeneralModel<X>> type, but with IModelSnapshot<X> its fine)
            if (iModelSnapshotType is not null)
            {
                var toGenericListModelMethod = typeof(GeneralListModelExtensions).GetMethod("ToGeneralListModel", BindingFlags.Static | BindingFlags.Public)!;
                var toGenericListModelGenericMethod = toGenericListModelMethod.MakeGenericMethod(new Type[] { iModelSnapshotType });
                childList1 = (IEnumerable<IModelDistinguishable>)toGenericListModelGenericMethod.Invoke(null, new object[] { childList1 })!;
            }

            var getListDifference = childList1.GetType().GetMethods()
                .Where(m => m.Name == nameof(IListModelDistinguishable.GetListDifference))
                .Select(m => new
                {
                    Method = m,
                    Params = m.GetParameters()
                })
                .Where(x => x.Params.Length == 1)
                .OrderByDescending(x => x.Params[0].ParameterType.GetGenericArguments().Length)
                .Select(x => x.Method)
                .First();

            object[] methodParameters = { childList2 };
            return (ListDifference?)getListDifference.Invoke(childList1, methodParameters);
        }
    }
}
