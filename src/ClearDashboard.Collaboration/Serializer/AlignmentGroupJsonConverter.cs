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

        alignmentGroup.AlignmentSetId = ReadGuidProperty(ref reader, nameof(Models.Alignment.AlignmentSetId));

        ReadPropertyName(ref reader, nameof(Models.AlignmentSet.ParallelCorpus.SourceTokenizedCorpus));
        alignmentGroup.SourceTokenizedCorpus = JsonSerializer.Deserialize<TokenizedCorpusExtra>(ref reader, options)!;
        
        ReadPropertyName(ref reader, nameof(Models.AlignmentSet.ParallelCorpus.TargetTokenizedCorpus));
        alignmentGroup.TargetTokenizedCorpus = JsonSerializer.Deserialize<TokenizedCorpusExtra>(ref reader, options)!;

        alignmentGroup.Location = ReadStringProperty(ref reader, AlignmentBuilder.BOOK_CHAPTER_LOCATION)!;

        ReadStartArray(ref reader, nameof(AlignmentGroup.Alignments));
        alignmentGroup.Alignments = new GeneralListModel<GeneralModel<Alignment>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;
            if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }

            var properties = new Dictionary<string, object?>();

            properties.Add("Ref", ReadStringProperty(ref reader, "Ref"));
            properties.Add(nameof(Models.Alignment.AlignmentSetId), alignmentGroup.AlignmentSetId);
            properties.Add(AlignmentBuilder.SOURCE_TOKEN_LOCATION, ReadStringProperty(ref reader, AlignmentBuilder.SOURCE_TOKEN_LOCATION));
            properties.Add(AlignmentBuilder.TARGET_TOKEN_LOCATION, ReadStringProperty(ref reader, AlignmentBuilder.TARGET_TOKEN_LOCATION));
            properties.Add(nameof(Models.Alignment.AlignmentOriginatedFrom), ReadEnumProperty<AlignmentOriginatedFrom>(ref reader, nameof(Models.Alignment.AlignmentOriginatedFrom)));
            properties.Add(nameof(Models.Alignment.AlignmentVerification), ReadEnumProperty<AlignmentVerification>(ref reader, nameof(Models.Alignment.AlignmentVerification)));
            properties.Add(nameof(Models.Alignment.Created), ReadDateTimeOffsetProperty(ref reader, nameof(Models.Alignment.Created)));
            properties.Add(nameof(Models.Alignment.Deleted), ReadDateTimeOffsetProperty(ref reader, nameof(Models.Alignment.Deleted)));
            properties.Add(nameof(Models.Alignment.Score), ReadDoubleProperty(ref reader, nameof(Models.Alignment.Score)));
            properties.Add(nameof(Models.Alignment.UserId), ReadGuidProperty(ref reader, nameof(Models.Alignment.UserId)));
            properties.Add(AlignmentBuilder.BOOK_CHAPTER_LOCATION, alignmentGroup.Location);

            var alignment = new GeneralModel<Models.Alignment>("Ref", properties);

            alignmentGroup.Alignments.Add(alignment);

            ReadEndObject(ref reader);
        }

        CheckEndArray(ref reader);
        ReadEndObject(ref reader);

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
        writer.WriteStartArray(nameof(AlignmentGroup.Alignments));

        foreach (var alignment in value.Alignments)
        {
            writer.WriteStartObject();
            writer.WriteString("Ref", (string)alignment["Ref"]!);
            writer.WriteString(AlignmentBuilder.SOURCE_TOKEN_LOCATION, (string)alignment[AlignmentBuilder.SOURCE_TOKEN_LOCATION]!);
            writer.WriteString(AlignmentBuilder.TARGET_TOKEN_LOCATION, (string)alignment[AlignmentBuilder.TARGET_TOKEN_LOCATION]!);
            writer.WriteString(nameof(Models.Alignment.AlignmentOriginatedFrom), alignment[nameof(Models.Alignment.AlignmentOriginatedFrom)]!.ToString());
            writer.WriteString(nameof(Models.Alignment.AlignmentVerification), alignment[nameof(Models.Alignment.AlignmentVerification)]!.ToString());
            
            writer.WriteString(nameof(Models.Alignment.Created), (DateTimeOffset)alignment[nameof(Models.Alignment.Created)]!);
            if (alignment[nameof(Models.Alignment.Deleted)] != null)
            {
                writer.WriteString(nameof(Models.Alignment.Deleted), (DateTimeOffset)alignment[nameof(Models.Alignment.Deleted)]!);
            }
            else
            {
                writer.WriteNull(nameof(Models.Alignment.Deleted));
            }
            writer.WriteNumber(nameof(Models.Alignment.Score), (double)alignment[nameof(Models.Alignment.Score)]!);
            writer.WriteString(nameof(Models.Alignment.UserId), alignment[nameof(Models.Alignment.UserId)]!.ToString());

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void ReadPropertyName(ref Utf8JsonReader reader, string propertyName)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != propertyName)
        {
            throw new JsonException($"Expected property name '{propertyName}'");
        }
    }

    private static void ReadStartArray(ref Utf8JsonReader reader, string propertyName)
    {
        ReadPropertyName(ref reader, propertyName);

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException($"Expected 'StartArray' for start of '{propertyName}' items"); }
    }

    private static void CheckEndArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException($"Expected 'EndArray'"); }
    }

    private static void ReadStartObject(ref Utf8JsonReader reader, string propertyName)
    {
        ReadPropertyName(ref reader, propertyName);

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject' for start of '{propertyName}' properties"); }
    }

    private static void ReadStartObject(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }
    }

    private static void ReadEndObject(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject'"); }
    }

    private static T ReadEnumProperty<T>(ref Utf8JsonReader reader, string propertyName) where T: struct
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String);
        var stringValue = reader.GetString();

        if (Enum.TryParse(stringValue, out T enumValue)) return enumValue;

        throw new SerializedDataException($"Expected enum '{typeof(T).Name}' value for PropertyName '{propertyName}'");
    }

    private static string? ReadStringProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String, JsonTokenType.Null);
        if (reader.TokenType == JsonTokenType.Null) return null;
        return reader.GetString();
    }

    private static Guid ReadGuidProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String);
        if (reader.TryGetGuid(out var value)) return value;

        throw new SerializedDataException($"Expected int value for PropertyName '{propertyName}'");
    }

    private static int ReadIntProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.Number);
        if (reader.TryGetInt32(out int value)) return value;

        throw new SerializedDataException($"Expected int value for PropertyName '{propertyName}'");
    }

    private static double ReadDoubleProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.Number);
        if (reader.TryGetDouble(out double value)) return value;

        throw new SerializedDataException($"Expected double value for PropertyName '{propertyName}'");
    }

    private static bool ReadEnsureBoolProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.True, JsonTokenType.False);
        return reader.GetBoolean();
    }

    private static DateTimeOffset? ReadDateTimeOffsetProperty(ref Utf8JsonReader reader, string propertyName)
    {
        ReadEnsurePropertyNameType(ref reader, propertyName, JsonTokenType.String, JsonTokenType.Null);
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TryGetDateTimeOffset(out var value)) return value;

        throw new SerializedDataException($"Expected DateTimeOffset value for PropertyName '{propertyName}'");
    }

    private static void ReadEnsurePropertyNameType(ref Utf8JsonReader reader, string propertyName, params JsonTokenType[] expectedTypes)
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