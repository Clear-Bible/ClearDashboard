using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClearDashboard.Collaboration.Serializer;

public class SystemTypeJsonConverter : JsonConverter<Type>
{
    public override Type Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        Assembly asm = typeof(GeneralModel).Assembly;
        string? typeString = reader.GetString();
        Type? type = asm?.GetType($"{typeString}");
        return type ?? throw new ArgumentException($"Unable to deserialize type {typeString}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        Type typeValue,
        JsonSerializerOptions options) =>
            writer.WriteStringValue(typeValue.IsGenericType ? typeValue.ShortDisplayName() : typeValue.FullName ?? typeValue.Name);
}

