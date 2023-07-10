using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearDashboard.DataAccessLayer.Models;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Notes;

public class LabelGroupDataJsonConverter : JsonConverter<IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>
{
    public override IDictionary<string, IEnumerable<(string Text, string? TemplateText)>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var labelGroupData = new Dictionary<string, IEnumerable<(string Text, string? TemplateText)>>();

        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException("Expected 'StartArray' (LabelGroups array)"); }
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;
            if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject' for LabelGroup data"); }

            var labelGroupLabelData = new List<(string Text, string? TemplateText)>();

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (LabelGroup:Name)"); }
            reader.Read();
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string property (LabelGroup:Name property value)"); }

            var labelGroupName = reader.GetString();
            if (string.IsNullOrEmpty(labelGroupName)) { throw new JsonException($"LabelGroup:Name string value is null or empty"); }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (LabelGroup:Labels)"); }
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException("Expected 'StartArray' (LabelGroup:Labels array)"); }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) break;
                if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject' for Label data"); }

                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (Label:Text)"); }
                reader.Read();
                if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string property (Label:Text property value)"); }

                var labelText = reader.GetString();
                if (string.IsNullOrEmpty(labelText)) { throw new JsonException($"Label:Text string value is null or empty"); }

                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (Label:TemplateText)"); }
                reader.Read();
                if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.Null) 
                    { throw new JsonException($"Expected string property or null (Label:TemplateText property value)"); }

                var labelTemplateText = reader.GetString();

                labelGroupLabelData.Add((Text: labelText, TemplateText: labelTemplateText));

                reader.Read();
                if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject' for Label data"); }
            }

            labelGroupData.Add(labelGroupName, labelGroupLabelData);

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject' for LabelGroup data"); }
        }

        return labelGroupData;
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<string, IEnumerable<(string Text, string? TemplateText)>> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var kvp in value)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", kvp.Key);
            writer.WriteStartArray("Labels");
            
            foreach (var labelData in kvp.Value)
            {
                writer.WriteStartObject();
                writer.WriteString("Text", labelData.Text);
                writer.WriteString("TemplateText", labelData.TemplateText);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}