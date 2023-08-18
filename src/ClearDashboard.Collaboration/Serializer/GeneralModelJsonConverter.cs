using System;
using System.Data.Entity.Core;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using Microsoft.Extensions.Options;
using SIL.Machine.Clusterers;
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
        return DeserializeGeneralModel(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, GeneralModel value, JsonSerializerOptions options)
    {
        var modelBuilder = GeneralModelBuilder.GetGeneralModelBuilder(value.GetType());
        if (modelBuilder.NoSerializePropertyNames.Any())
        {
            JsonSerializer.Serialize(
                writer,
                value.PropertyValues.ExceptBy(modelBuilder.NoSerializePropertyNames, p => p.Key).ToDictionary(p => p.Key, p => p.Value),
                options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.PropertyValues, options);
        }
    }

    public static GeneralModel DeserializeGeneralModel(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElements = (Dictionary<string, object?>?)JsonSerializer.Deserialize(ref reader, typeof(Dictionary<string, object?>), options);
        if (jsonElements is null)
        {
            throw new JsonException($"Unable to deserialize GeneralModel Properties");
        }

        var modelBuilder = GeneralModelBuilder.GetGeneralModelBuilder(typeToConvert);
        var modelPropertiesTypes = new Dictionary<string, (Type type, object? value)>();

        var propertyInfos = modelBuilder.PropertyInfos.ToDictionary(p => p.Name, p => p.PropertyType);
        foreach (var kvp in jsonElements)
        {
            Type? propertyType = null;
            if (!propertyInfos.TryGetValue(kvp.Key, out propertyType) &&
                !modelBuilder.AddedPropertyNamesTypes.TryGetValue(kvp.Key, out propertyType))
            {
                var exceptionString = $"Unexpected property name '{kvp.Key}' encountered when building GeneralModel from property dictionary";
                exceptionString += "\n\nPlease update your version of ClearDashboard to the latest version";

                throw new ArgumentException(exceptionString);
            }

            if (kvp.Value is null)
            {
                modelPropertiesTypes.Add(kvp.Key, (propertyType, kvp.Value));
            }
            else
            {
                modelPropertiesTypes.Add(
                kvp.Key,
                    (propertyType, ((JsonElement)kvp.Value).Deserialize(propertyType, options)));
            }
        }

        return modelBuilder.BuildGeneralModel(modelPropertiesTypes);
    }

    public static void ReadPropertyName(ref Utf8JsonReader reader, string propertyName)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != propertyName)
        {
            throw new JsonException($"Expected property name '{propertyName}'");
        }
    }

    public static void ReadStartArray(ref Utf8JsonReader reader, string propertyName)
    {
        ReadPropertyName(ref reader, propertyName);

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException($"Expected 'StartArray' for start of '{propertyName}' items"); }
    }

    public static void CheckEndArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException($"Expected 'EndArray'"); }
    }

    public static void ReadStartObject(ref Utf8JsonReader reader, string propertyName)
    {
        ReadPropertyName(ref reader, propertyName);

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject' for start of '{propertyName}' properties"); }
    }

    public static void ReadStartObject(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }
    }

    public static void ReadEndObject(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject'"); }
    }

    public static T ReadEnumProperty<T>(ref Utf8JsonReader reader, string propertyName) where T : struct
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String);
        var stringValue = reader.GetString();

        if (Enum.TryParse(stringValue, out T enumValue)) return enumValue;

        throw new SerializedDataException($"Expected enum '{typeof(T).Name}' value for PropertyName '{propertyName}'");
    }

    public static string? ReadStringProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String, JsonTokenType.Null);
        if (reader.TokenType == JsonTokenType.Null) return null;
        return reader.GetString();
    }

    public static Guid ReadGuidProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String);
        if (reader.TryGetGuid(out var value)) return value;

        throw new SerializedDataException($"Expected int value for PropertyName '{propertyName}'");
    }

    public static int ReadIntProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.Number);
        if (reader.TryGetInt32(out int value)) return value;

        throw new SerializedDataException($"Expected int value for PropertyName '{propertyName}'");
    }

    public static double ReadDoubleProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.Number);
        if (reader.TryGetDouble(out double value)) return value;

        throw new SerializedDataException($"Expected double value for PropertyName '{propertyName}'");
    }

    public static bool ReadEnsureBoolProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.True, JsonTokenType.False);
        return reader.GetBoolean();
    }

    public static DateTimeOffset? ReadDateTimeOffsetProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String, JsonTokenType.Null);
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TryGetDateTimeOffset(out var value)) return value;

        throw new SerializedDataException($"Expected DateTimeOffset value for PropertyName '{propertyName}'");
    }

    public static void ReadEnsurePropertyNameType(ref Utf8JsonReader reader, string propertyName, params JsonTokenType[] expectedTypes)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != propertyName)
        {
            throw new SerializedDataException($"Expected token type '{JsonTokenType.PropertyName.ToString()}' with value '{propertyName}'");
        }

        reader.Read();
        if (!expectedTypes.Contains(reader.TokenType)) { throw new JsonException($"Expected one of token types: '{string.Join(", ", expectedTypes.Select(e => e.ToString()))}' value for property '{propertyName}'"); }
    }
}