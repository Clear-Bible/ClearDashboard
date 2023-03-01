using System;
using System.Data.Entity.Core;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Builder;
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
                throw new ArgumentException($"Unexpected property name '{kvp.Key}' encountered when building GeneralModel from property dictionary");
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

    public override void Write(Utf8JsonWriter writer, GeneralModel value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.PropertyValues, options);
    }
}