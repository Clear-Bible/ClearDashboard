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

public class AlignmentGroupJsonConverter : JsonConverter<AlignmentGroup>
{
    public override AlignmentGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var alignmentGroup = new AlignmentGroup();

        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject'"); }

        alignmentGroup.AlignmentSetId = GeneralModelJsonConverter.ReadGuidProperty(ref reader, nameof(Models.Alignment.AlignmentSetId));

        GeneralModelJsonConverter.ReadPropertyName(ref reader, nameof(Models.AlignmentSet.ParallelCorpus.SourceTokenizedCorpus));
        alignmentGroup.SourceTokenizedCorpus = JsonSerializer.Deserialize<TokenizedCorpusExtra>(ref reader, options)!;

        GeneralModelJsonConverter.ReadPropertyName(ref reader, nameof(Models.AlignmentSet.ParallelCorpus.TargetTokenizedCorpus));
        alignmentGroup.TargetTokenizedCorpus = JsonSerializer.Deserialize<TokenizedCorpusExtra>(ref reader, options)!;

        alignmentGroup.Location = GeneralModelJsonConverter.ReadStringProperty(ref reader, AlignmentBuilder.BOOK_CHAPTER_LOCATION)!;

        GeneralModelJsonConverter.ReadStartArray(ref reader, "Alignments");
        alignmentGroup.Items = new GeneralListModel<GeneralModel<Alignment>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;
            if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }

            var alignment = GeneralModelJsonConverter.DeserializeGeneralModel(ref reader, typeof(GeneralModel<Models.Alignment>), options);
            alignment.Add(nameof(Models.Alignment.AlignmentSetId), alignmentGroup.AlignmentSetId);

            alignmentGroup.Items.Add((GeneralModel<Alignment>)alignment);
        }

        GeneralModelJsonConverter.CheckEndArray(ref reader);
        GeneralModelJsonConverter.ReadEndObject(ref reader);

        return alignmentGroup;
    }

    public override void Write(Utf8JsonWriter writer, AlignmentGroup value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Models.Alignment.AlignmentSetId), value.AlignmentSetId.ToString());
        writer.WritePropertyName(nameof(Models.AlignmentSet.ParallelCorpus.SourceTokenizedCorpus));
        JsonSerializer.Serialize(writer, value.SourceTokenizedCorpus, options);
        writer.WritePropertyName(nameof(Models.AlignmentSet.ParallelCorpus.TargetTokenizedCorpus));
        JsonSerializer.Serialize(writer, value.TargetTokenizedCorpus, options);
        writer.WriteString(AlignmentBuilder.BOOK_CHAPTER_LOCATION, value.Location);

        writer.WriteStartArray("Alignments");

        foreach (var alignment in value.Items)
        {
            JsonSerializer.Serialize(writer, alignment, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}