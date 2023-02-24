using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Serializer;

public class GeneralModelJsonConverter : JsonConverter<GeneralModel>
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(GeneralModel) || objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(GeneralModel<>);
    }

    public override GeneralModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var modelProperties = new Dictionary<string, object?>();
        var jsonElements = (Dictionary<string, object?>?)JsonSerializer.Deserialize(ref reader, typeof(Dictionary<string, object?>), options);

        if (jsonElements is null)
        {
            throw new JsonException($"Unable to deserialize GeneralModel Properties");
        }

        List<string> internalElementNames = new() { "$identityKey", "$type" };

        string identityKey = string.Empty;

        if (jsonElements.TryGetValue("$identityKey", out var identityKeyNameElement) &&
            identityKeyNameElement is JsonElement &&
            ((JsonElement)identityKeyNameElement).ValueKind == JsonValueKind.String)
        {
            identityKey = ((JsonElement)identityKeyNameElement).GetString()!;
        }
        else
        {
            throw new SerializedDataException($"Unable to determine identity key for deserialized GeneralModel from: {string.Join(Environment.NewLine, jsonElements)}");
        }

        Type? modelType = null;
        if (jsonElements.TryGetValue("$type", out var modelTypeName) &&
            modelTypeName is JsonElement &&
            ((JsonElement)modelTypeName).ValueKind == JsonValueKind.String)
        {
            var exampleType = typeof(Models.Corpus);
            Assembly asm = exampleType.Assembly;
            modelType = asm?.GetType($"{exampleType.Namespace}.{modelTypeName}");
        }

        if (modelType is null)
        {
            throw new SerializedDataException($"Unable to determine generic type for deserialized GeneralModel from '$type' value: {modelTypeName}");
        }

        var addedPropertyTypes = new Dictionary<string, string>();
        foreach (var kvp in jsonElements)
        {
            if (kvp.Key.StartsWith("$addedType_") && kvp.Value is not null)
            {
                var addedPropertyTypeName = ((JsonElement)kvp.Value).GetString();
                if (addedPropertyTypeName is not null)
                {
                    addedPropertyTypes.Add(
                        kvp.Key.Substring(kvp.Key.LastIndexOf("_") + 1),
                        addedPropertyTypeName
                    );
                }

                internalElementNames.Add(kvp.Key);
            }
        }

        foreach (var kvp in jsonElements)
        {
            if (internalElementNames.Contains(kvp.Key))
            {
                continue;
            }

            if (kvp.Value is null)
            {
                modelProperties.Add(kvp.Key, kvp.Value);
            }
            else if (modelType.TryGetModelSnapshotPropertyType(addedPropertyTypes, kvp.Key, out var modelPropertyType))
            {
                if (modelPropertyType is not null)
                {
                    modelProperties.Add(kvp.Key, ((JsonElement)kvp.Value).Deserialize(modelPropertyType, options));
                }
                else
                {
                    throw new SerializedDataException($"Unable to create type '{modelType.GetType().Name}' property '{kvp.Key}' to deserialize into");
                }
            }
            else
            {
                modelProperties.Add(kvp.Key, ((JsonElement)kvp.Value).GetString());
            }
        }

        return modelProperties.ToGeneralModel(identityKey, modelType, addedPropertyTypes);
    }

    public override void Write(Utf8JsonWriter writer, GeneralModel value, JsonSerializerOptions options)
    {
        var g = value.GetType().GetGenericArguments();
        if (g.Length > 0)
        {
            var properties = new Dictionary<string, object?>(value.PropertyValues) { { "$type", g[0].Name } };
            if (value.AddedPropertyTypeNames is not null && value.AddedPropertyTypeNames.Any())
            {
                foreach (var ot in value.AddedPropertyTypeNames)
                {
                    properties.Add($"$addedType_{ot.Key}", ot.Value);
                }
            }

            properties.Add("$identityKey", value.IdentityKey);

            JsonSerializer.Serialize(writer, properties, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.PropertyValues, options);
        }
    }
}