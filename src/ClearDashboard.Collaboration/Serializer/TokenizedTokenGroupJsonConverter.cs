using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Serializer;

public class TokenizedTokenGroupJsonConverter : JsonConverter<TokenizedTokenGroup>
{
    public override TokenizedTokenGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenizedTokenGroup = new TokenizedTokenGroup();

        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException("Expected 'StartObject'"); }

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName ||
            reader.GetString() != nameof(Models.Token.TokenizedCorpusId))
        {
            throw new JsonException($"Expected property name '{nameof(Models.Token.TokenizedCorpusId)}'");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string value for property '{nameof(Models.Token.TokenizedCorpusId)}'"); }
        var tokenizedCorpusId = Guid.Parse(reader.GetString()!);
        tokenizedTokenGroup.TokenizedCorpusId = tokenizedCorpusId;

        GeneralModelJsonConverter.ReadStartArray(ref reader, ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(TokenizedTokenGroup)].childName);
        tokenizedTokenGroup.Items = new GeneralListModel<GeneralModel<Models.Token>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;
            if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject'"); }

            var token = GeneralModelJsonConverter.DeserializeGeneralModel(ref reader, typeof(GeneralModel<Models.Token>), options);
            token.Add(nameof(Models.Token.TokenizedCorpusId), tokenizedTokenGroup.TokenizedCorpusId);

            tokenizedTokenGroup.Items.Add((GeneralModel<Models.Token>)token);
        }

        GeneralModelJsonConverter.CheckEndArray(ref reader);
        GeneralModelJsonConverter.ReadEndObject(ref reader);

        //reader.Read();
        //if (reader.TokenType != JsonTokenType.PropertyName ||
        //    reader.GetString() != ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(TokenizedTokenGroup)].childName)
        //{
        //    throw new JsonException($"Expected property name '{ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(TokenizedTokenGroup)].childName}'");
        //}

        //reader.Read();
        //if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException($"Expected 'StartObject' for start of Token list"); }

        //while (reader.Read())
        //{
        //    if (reader.TokenType == JsonTokenType.EndObject) break;

        //    if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException($"Expected property name (the token '{nameof(Models.Token.TrainingText)}')"); }
        //    var trainingText = reader.GetString();
        //    if (string.IsNullOrEmpty(trainingText)) { throw new JsonException("Property name containing '{nameof(Models.Token.TrainingText)}' is null or empty"); }

        //    reader.Read();
        //    if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"Expected string value (the token data)"); }
        //    var tokenData = reader.GetString();
        //    if (string.IsNullOrEmpty(tokenData)) { throw new JsonException("Property value containing the other token data is null or empty"); }

        //    string[] tokenDataParts = tokenData.Split(" | ");
        //    if (tokenDataParts.Length != 12) { throw new JsonException($"Expected token data string to contain 12 parts ({tokenDataParts.Length} found instead)"); }

        //    DateTimeOffset? deleted = string.IsNullOrEmpty(tokenDataParts[11])
        //        ? null
        //        : JsonSerializer.Deserialize<DateTimeOffset>(tokenDataParts[11], options);

        //    var index = string.IsNullOrEmpty(tokenDataParts[8])
        //        ? (int?)null
        //        : int.Parse(tokenDataParts[8]);

        //    var bookNumber = int.Parse(tokenDataParts[1]);
        //    var chapterNumber = int.Parse(tokenDataParts[2]);
        //    var verseNumber = int.Parse(tokenDataParts[3]);
        //    var wordNumber = int.Parse(tokenDataParts[4]);
        //    var subwordNumber = int.Parse(tokenDataParts[5]);
        //    var originTokenLocation = string.IsNullOrEmpty(tokenDataParts[7]) ? null : tokenDataParts[7];
        //    var engineTokenId = (new TokenId(
        //        bookNumber,
        //        chapterNumber,
        //        verseNumber,
        //        wordNumber,
        //        subwordNumber
        //    )).ToString();

        //    var properties = new Dictionary<string, (Type, object?)>
        //    {
        //        { nameof(Models.Token.TrainingText), (typeof(string), trainingText) },
        //        { nameof(Models.Token.SurfaceText), (typeof(string), tokenDataParts[0]) },
        //        { nameof(Models.Token.BookNumber), (typeof(int), bookNumber) },
        //        { nameof(Models.Token.ChapterNumber), (typeof(int), chapterNumber) },
        //        { nameof(Models.Token.VerseNumber), (typeof(int), verseNumber) },
        //        { nameof(Models.Token.WordNumber), (typeof(int), wordNumber) },
        //        { nameof(Models.Token.SubwordNumber), (typeof(int), subwordNumber) },
        //        { TokenBuilder.VERSE_ROW_LOCATION, (typeof(string), tokenDataParts[6]) },
        //        { nameof(Models.Token.EngineTokenId), (typeof(string), engineTokenId) },
        //        { nameof(Models.Token.OriginTokenLocation), (typeof(string), originTokenLocation) },
        //        { nameof(Models.Token.Type), string.IsNullOrEmpty(tokenDataParts[9]) ? (typeof(string), null) : (typeof(string), tokenDataParts[9]) },
        //        { nameof(Models.Token.ExtendedProperties), string.IsNullOrEmpty(tokenDataParts[10]) ? (typeof(string), null) : (typeof(string), tokenDataParts[10]) },
        //        { nameof(Models.Token.Deleted), (typeof(DateTimeOffset?), deleted) }
        //    };

        //    var tokenSnapshot = new GeneralModel<Models.Token>(
        //        TokenBuilder.BuildPropertyRefName(),
        //        TokenBuilder.CalculateRef(tokenizedCorpusId, engineTokenId, originTokenLocation, index)
        //    );
        //    TokenBuilder.AddPropertyValuesToGeneralModel(tokenSnapshot, properties);

        //    tokenizedTokenGroup.Items.Add(tokenSnapshot);
        //}

        //reader.Read();
        //if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException($"Expected 'EndObject'"); }

        return tokenizedTokenGroup;
    }

    public override void Write(Utf8JsonWriter writer, TokenizedTokenGroup value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Models.Token.TokenizedCorpusId), value.TokenizedCorpusId.ToString());

        writer.WriteStartArray(ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(TokenizedTokenGroup)].childName);

        foreach (var token in value.Items)
        {
            //var tokenRef = (string)token.GetId()!;
            //var hasIndex = int.TryParse(((string)tokenRef!).Split("_").LastOrDefault(), out int refIndex);
            //var index = hasIndex ? refIndex : (int?)null;

            //var trainingText = (string)token[nameof(Models.Token.TrainingText)]!;
            //var surfaceText = (string)token[nameof(Models.Token.SurfaceText)]!;
            //var bookNumber = (int)token[nameof(Models.Token.BookNumber)]!;
            //var chapterNumber = (int)token[nameof(Models.Token.ChapterNumber)]!;
            //var verseNumber = (int)token[nameof(Models.Token.VerseNumber)]!;
            //var wordNumber = (int)token[nameof(Models.Token.WordNumber)]!;
            //var subwordNumber = (int)token[nameof(Models.Token.SubwordNumber)]!;
            //var verseRowLocation = (string)(token[TokenBuilder.VERSE_ROW_LOCATION]!);
            //token.TryGetNullableStringPropertyValue(nameof(Models.Token.OriginTokenLocation), out string? originTokenLocation);
            //token.TryGetNullableStringPropertyValue(nameof(Models.Token.Type), out string? type);
            //token.TryGetNullableStringPropertyValue(nameof(Models.Token.ExtendedProperties), out string? extendedProperties);

            //var deleted = (DateTimeOffset?)token[nameof(Models.Token.Deleted)]!;
            //var deletedSerialized = (deleted is not null) ? JsonSerializer.Serialize(deleted, options) : null;

            //writer.WriteString(trainingText, $"{surfaceText} | {bookNumber} | {chapterNumber} | {verseNumber} | {wordNumber} | {subwordNumber} | {verseRowLocation} | {originTokenLocation} | {index} | {type} | {extendedProperties} | {deletedSerialized}");

            JsonSerializer.Serialize(writer, token, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}