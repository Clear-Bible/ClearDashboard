using System;
using System.Security.Cryptography;
using System.Text;
using ClearDashboard.Collaboration.DifferenceModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.Collaboration.Exceptions;
using SIL.Machine.Tokenization;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.Collaboration.Model;

internal static class GeneralModelExtensions
{
    internal static PropertyInfo? GetIdentityProperty(this Type entityType)
    {
        return entityType.GetProperties()
            .Select(p => new { property = p, dgo = ((DatabaseGeneratedAttribute?)p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute), true))?.DatabaseGeneratedOption })
            .Where(p => p.dgo != null && p.dgo.HasValue && p.dgo.Value == DatabaseGeneratedOption.Identity)
            .Select(p => p.property)
            .FirstOrDefault();
    }

    internal static IEnumerable<PropertyInfo> GetForeignKeyProperties(this Type entityType)
    {
        var propertyNames = entityType.GetProperties()
            .Select(p => ((ForeignKeyAttribute?)p.GetCustomAttribute(typeof(ForeignKeyAttribute), true))?.Name)
            .Where(p => p != null)
            .ToList();

        return entityType.GetProperties()
            .Where(p => propertyNames.Contains(p.Name))
            .ToList();
    }

    internal static bool IsDatabasePrimitiveType(this Type type)
    {
        // Leaving out ancillary Dictionary database columns like 'Metadata'
        // If at some point we need to include Dictionaries, we'll have to
        // add support in GeneralModel as well as determining differences.
        return
            type.IsValueType ||
            type == typeof(string) /* ||
            type == typeof(Dictionary<string, object>) ||
            type == typeof(Dictionary<string, object?>)*/;
    }

    internal static GeneralModel ToGeneralModel(
        this IEnumerable<KeyValuePair<string, object?>> modelProperties,
        string identityKey,
        Type modelType,
        Dictionary<string, string>? addedPropertyTypes = null)
    {
        Type[] typeArgs = { modelType };
        Type genericType = typeof(GeneralModel<>).MakeGenericType(typeArgs);

        Dictionary<string, object?> properties = (modelProperties is Dictionary<string, object?>) ?
            (Dictionary<string, object?>)modelProperties :
            modelProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        object?[] constructorArgs = { identityKey, properties, addedPropertyTypes, null };
        return (GeneralModel)Activator.CreateInstance(genericType, constructorArgs)!;
    }

    internal static bool TryGetModelSnapshotPropertyType(this Type modelType, IReadOnlyDictionary<string, string>? addedPropertyTypeNames, string propertyName, out Type? type)
    {
        type = null;

        if (addedPropertyTypeNames is not null && addedPropertyTypeNames.TryGetValue(propertyName, out var addedPropertyTypeName))
        {
            type = addedPropertyTypeName.ModelSnapshotTypeNameToType();
        }
        else
        {
            type = modelType.GetProperty(propertyName)?.PropertyType;
            //if (type == typeof(Dictionary<string, object>))
            //{
            //    // Used for Metadata:
            //    type = typeof(GeneralDictionaryModel<string, object>);
            //}
        }

        return type is not null;
    }

    internal static string ModelSnapshotTypeNameFromType(this Type type)
    {
        return type.IsGenericType ? type.ShortDisplayName() : type.FullName ?? type.Name;
    }

    internal static Type ModelSnapshotTypeNameToType(this string modelSnapshotTypeName)
    {
        // Per the GeneralModel Add (property) methods, we only accept:
        //  string?,
        //  ValueType?,
        //  ModelRef,
        //  GeneralListModel<string>,
        //  GeneralDictionaryModel<string, object>
        // And per ModelSnapshotTypeNameFromType, we store the ShortDisplayName for generics and FullName for others
        if (modelSnapshotTypeName == "GeneralListModel<string>")
        {
            return typeof(GeneralListModel<string>);
        }
        //else if (modelSnapshotTypeName == "GeneralDictionaryModel<string, object>")
        //{
        //    return typeof(GeneralDictionaryModel<string, object>);
        //}
        else
        {
            // Should cover:  ModelRef types, ValueTypes and string
            var type = Type.GetType(modelSnapshotTypeName);
            if (type is null)
            {
                throw new SerializedDataException($"Unable to convert serializable type name {modelSnapshotTypeName} to type");
            }

            return type;
        }
    }

    internal static IModelDifference<IModelSnapshot<T>> FindModelDifference<T>(this IModelSnapshot<T> modelSnapshot, IModelSnapshot<T> other)
        where T: notnull
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
        where T: notnull 
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

    internal static ListDifference? FindChildListDifference<T>(this IModelSnapshot<T> modelSnapshot, string childName, IModelSnapshot<T> otherModelSnapshot)
        where T: notnull
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

    internal static string ToMD5String(this string sourceString)
    {
        var bytes = ASCIIEncoding.ASCII.GetBytes(sourceString);
        var hashBytes = MD5.HashData(bytes);

        int i;
        StringBuilder sOutput = new StringBuilder(hashBytes.Length);
        for (i = 0; i < hashBytes.Length; i++)
        {
            sOutput.Append(hashBytes[i].ToString("X2"));
        }
        return sOutput.ToString();
    }

    internal static ITokenizer<string, int, string> ToTokenizer(this string tokenizerString)
    {
        var assemblyTokenizerType = typeof(LatinWordTokenizer);
        var assembly = assemblyTokenizerType!.Assembly;
        var tokenizerType = assembly.GetType($"{assemblyTokenizerType.Namespace}.{tokenizerString}");

        if (tokenizerType is null)
        {
            throw new ArgumentException($"Tokenizer '{tokenizerString}' not a valid class in the '{assemblyTokenizerType.Namespace}' namespace");
        }

        var tokenizer = (ITokenizer<string, int, string>)Activator.CreateInstance(tokenizerType)!;
        return tokenizer;
    }

}

