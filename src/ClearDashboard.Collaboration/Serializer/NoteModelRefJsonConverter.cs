using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Serializer;

public class NoteModelRefJsonConverter : JsonConverter<NoteModelRef>
{
    public override bool CanConvert(Type objectType)
    {
        return
            objectType.IsAssignableTo(typeof(NoteModelRef)) || objectType.IsGenericType && objectType.IsAssignableToGenericType(typeof(NoteModelRef<>));
    }

    public override NoteModelRef Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElements = (Dictionary<string, object?>?)JsonSerializer.Deserialize(ref reader, typeof(Dictionary<string, object?>), options);
        if (jsonElements is null)
        {
            throw new JsonException($"Unable to deserialize NoteModelRef Properties");
        }

        if (jsonElements.TryGetValue(nameof(NoteDomainEntityAssociationId), out var ndeIdElement) &&
            jsonElements.TryGetValue(nameof(NoteId), out var noteIdElement) &&
            jsonElements.TryGetValue(nameof(ModelRef), out var modelRefElement))
        {
            if (ndeIdElement is not null && noteIdElement is not null && modelRefElement is not null)
            {
                var noteDomainEntityAssociationId = (Guid?)((JsonElement)ndeIdElement).Deserialize(typeof(Guid), options);
                var noteId = (Guid?)((JsonElement)noteIdElement).Deserialize(typeof(Guid), options);
                var modelRef = (ModelRef?)((JsonElement)modelRefElement).Deserialize(typeof(ModelRef), options);

                if (noteDomainEntityAssociationId is not null &&
                    noteId is not null &&
                    modelRef is not null)
                {
                    Type[] typeArgs = { modelRef.GetType() };
                    Type genericType = typeof(NoteModelRef<>).MakeGenericType(typeArgs);
                    object[] constructorArgs = { noteDomainEntityAssociationId ,noteId, modelRef };

                    return (NoteModelRef)Activator.CreateInstance(genericType, constructorArgs)!;
                }
            }
        }

        var asString = string.Join(Environment.NewLine, jsonElements);
        throw new ArgumentException($"Unable to determine generic type for deserialized NoteModelRef from: {asString}");
    }

    public override void Write(Utf8JsonWriter writer, NoteModelRef value, JsonSerializerOptions options)
    {
    }
}