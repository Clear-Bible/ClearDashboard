using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Serializer;

public class VerseRowModelListJsonConverter : JsonConverter<GeneralListModel<GeneralModel<Models.VerseRow>>>
{
    public override GeneralListModel<GeneralModel<Models.VerseRow>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var verseRowsForBook = new GeneralListModel<GeneralModel<Models.VerseRow>>();

        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject'"); }

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != "TokenizedCorpusId")
        {
            throw new JsonException($"Expected property name 'TokenizedCorpusId'");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string value for property 'TokenizedCorpusId'"); }
        var tokenizedCorpusId = Guid.Parse(reader.GetString()!);

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != "VerseRows")
        {
            throw new JsonException($"Expected property name 'VerseRows'");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject' for start of VerseRow list"); }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (the verse row bcv)"); }
            var bookChapterVerse = reader.GetString();
            if (string.IsNullOrEmpty(bookChapterVerse)) { throw new JsonException("Property name containing bookChapterVerse is null or empty"); }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string value (the verse row data)"); }
            var verseRowData = reader.GetString();
            if (string.IsNullOrEmpty(verseRowData)) { throw new JsonException("Property value containing the other verse row data is null or empty"); }

            string[] verseRowDataParts = verseRowData.Split(" | ");
            if (verseRowDataParts.Length != 8) { throw new JsonException($"Expected verse row data string to contain 8 parts ({verseRowDataParts.Length} found instead)"); }

            var properties = new Dictionary<string, object?>();

            properties.Add(nameof(Models.VerseRow.BookChapterVerse), bookChapterVerse);
            properties.Add(nameof(Models.VerseRow.OriginalText), verseRowDataParts[0]);
            properties.Add(nameof(Models.VerseRow.IsSentenceStart), bool.Parse(verseRowDataParts[1]));
            properties.Add(nameof(Models.VerseRow.IsInRange), bool.Parse(verseRowDataParts[2]));
            properties.Add(nameof(Models.VerseRow.IsRangeStart), bool.Parse(verseRowDataParts[3]));
            properties.Add(nameof(Models.VerseRow.IsEmpty), bool.Parse(verseRowDataParts[4]));
            properties.Add(nameof(Models.VerseRow.Created), DateTimeOffset.Parse(verseRowDataParts[5]));
            properties.Add(nameof(Models.VerseRow.Modified), string.IsNullOrEmpty(verseRowDataParts[6]) ? null : DateTimeOffset.Parse(verseRowDataParts[6]));
            properties.Add(nameof(Models.VerseRow.UserId), Guid.Parse(verseRowDataParts[7]));

            var verseRow = new GeneralModel<Models.VerseRow>(
                nameof(Models.VerseRow.BookChapterVerse),
                properties
            );
            verseRow.Add("TokenizedCorpusId", tokenizedCorpusId);
            verseRowsForBook.Add(verseRow);
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject'"); }

        return verseRowsForBook;
    }
    public override void Write(Utf8JsonWriter writer, GeneralListModel<GeneralModel<Models.VerseRow>> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Any())
        {
            if (value.First().TryGetPropertyValue("TokenizedCorpusId", out var tokenizedCorpusId))
            {
                if (tokenizedCorpusId is null || ((Guid)tokenizedCorpusId) == Guid.Empty)
                {
                    throw new InvalidModelStateException("Unable to extract tokenizedCorpusId for serializing GeneralModel VerseRows");
                }

                writer.WriteString("TokenizedCorpusId", tokenizedCorpusId.ToString());
                writer.WriteStartObject("VerseRows");
                foreach (var verseRow in value)
                {
                    var bcv = (string)verseRow["BookChapterVerse"]!;
                    var originalText = (string)(verseRow["OriginalText"] ?? "(null)");
                    var isSentenceStart = (bool)verseRow["IsSentenceStart"]!;
                    var isInRange = (bool)verseRow["IsInRange"]!;
                    var isRangeStart = (bool)verseRow["IsRangeStart"]!;
                    var isEmpty = (bool)verseRow["IsEmpty"]!;
                    var created = (DateTimeOffset)verseRow["Created"]!;
                    var modified = (DateTimeOffset?)verseRow["Modified"]!;
                    var userId = (Guid)verseRow["UserId"]!;

                    writer.WriteString(bcv, $"{originalText} | {isSentenceStart} | {isInRange} | {isRangeStart} | {isEmpty} | {created} | {modified} | {userId}");
                }
                writer.WriteEndObject();
            }
        }
        writer.WriteEndObject();
    }
}