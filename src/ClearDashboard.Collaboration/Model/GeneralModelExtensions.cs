using System;
using System.Security.Cryptography;
using System.Text;
using ClearDashboard.Collaboration.DifferenceModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.Exceptions;
using SIL.Machine.Tokenization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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

    internal static IEnumerable<PropertyInfo> GetMappedPrimitiveProperties(this Type entityType)
    {
        return entityType.GetProperties()
            .Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute), true) == null)
            .Where(p => IsDatabasePrimitiveType(p.PropertyType))
            .Where(p => p != null)
            .ToList();
    }

    internal static bool IsDatabasePrimitiveType(this Type type)
    {
        return
            type.IsValueType ||
            type == typeof(string) ||
            type == typeof(Dictionary<string, object>) ||
            type == typeof(Dictionary<string, object?>);
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

    internal static IEnumerable<string> ExtractAllTokenLocations(this IEnumerable<IModelSnapshot<DataAccessLayer.Models.Token>> tokenSnapshots)
    {
        var combined = new List<string>();
        if (tokenSnapshots.Any())
        {
            foreach (var tokenSnapshot in tokenSnapshots)
            {
                if (tokenSnapshot.TryGetStringPropertyValue(nameof(DataAccessLayer.Models.Token.EngineTokenId), out var engineTokenId))
                {
                    combined.Add(engineTokenId);
                }

                if (tokenSnapshot.TryGetStringPropertyValue(nameof(DataAccessLayer.Models.Token.OriginTokenLocation), out var originTokenLocation))
                {
                    combined.Add(originTokenLocation!);
                }
            }
        }

        return combined.Distinct();
    }
}

