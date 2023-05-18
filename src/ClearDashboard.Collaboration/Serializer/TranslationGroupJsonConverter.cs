using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Serializer;

public class TranslationGroupJsonConverter : JsonConverter<TranslationGroup>
{
    public override TranslationGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var translationGroup = new TranslationGroup();

        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject'"); }

        translationGroup.TranslationSetId = GeneralModelJsonConverter.ReadGuidProperty(ref reader, nameof(Models.Translation.TranslationSetId));

        GeneralModelJsonConverter.ReadPropertyName(ref reader, nameof(Models.TranslationSet.ParallelCorpus.SourceTokenizedCorpus));
        translationGroup.SourceTokenizedCorpus = JsonSerializer.Deserialize<TokenizedCorpusExtra>(ref reader, options)!;

        translationGroup.Location = GeneralModelJsonConverter.ReadStringProperty(ref reader, TranslationBuilder.BOOK_CHAPTER_LOCATION)!;

        GeneralModelJsonConverter.ReadStartArray(ref reader, "Translations");
        translationGroup.Items = new GeneralListModel<GeneralModel<Translation>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;
            if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }

            var translation = GeneralModelJsonConverter.DeserializeGeneralModel(ref reader, typeof(GeneralModel<Models.Translation>), options);
            translation.Add(nameof(Models.Translation.TranslationSetId), translationGroup.TranslationSetId);

            translationGroup.Items.Add((GeneralModel<Translation>)translation);
        }

        GeneralModelJsonConverter.CheckEndArray(ref reader);
        GeneralModelJsonConverter.ReadEndObject(ref reader);

        return translationGroup;
    }

    public override void Write(Utf8JsonWriter writer, TranslationGroup value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Models.Translation.TranslationSetId), value.TranslationSetId.ToString());
        writer.WritePropertyName(nameof(Models.TranslationSet.ParallelCorpus.SourceTokenizedCorpus));
        JsonSerializer.Serialize(writer, value.SourceTokenizedCorpus, options);
        writer.WriteString(TranslationBuilder.BOOK_CHAPTER_LOCATION, value.Location);

        writer.WriteStartArray("Translations");

        foreach (var translation in value.Items)
        {
            JsonSerializer.Serialize(writer, translation, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}