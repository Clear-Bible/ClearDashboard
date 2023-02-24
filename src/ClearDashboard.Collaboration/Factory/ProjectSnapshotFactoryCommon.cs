using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Factory;

public class ProjectSnapshotFactoryCommon
{
    public const string PROPERTIES_FILE = "_Properties";
    public static readonly string ProjectFolderNameTemplate = $"Project_{{0}}";
    public static readonly string VerseRowByBookFileNameTemplate = $"VerseRow_{{0}}";

    public static string ToProjectFolderName(Guid projectId) =>
        string.Format(ProjectFolderNameTemplate, projectId);
    public static string ToProjectPath(string parentPath, Guid projectId) =>
        Path.Combine(parentPath, string.Format(ProjectFolderNameTemplate, projectId));

    public static readonly Dictionary<Type, string> topLevelEntityFolderNameMappings = new() {
        { typeof(Models.AlignmentSet), "AlignmentSets" },
        { typeof(Models.Corpus), "Corpora" },
        { typeof(Models.Label), "Labels" },
        { typeof(Models.Note), "Notes" },
        { typeof(Models.TokenizedCorpus), "TokenizedCorpora" },
        { typeof(Models.ParallelCorpus), "ParallelCorpora" },
        { typeof(Models.TranslationSet), "TranslationSets" }
    };

    public static readonly Dictionary<Type, (string folderName, string childName)> childFolderNameMappings = new() {
        { typeof(Models.Alignment), ("Alignments", "Alignments") },
        { typeof(Models.TokenComposite), ("CompositeTokens", "CompositeTokens") },
        { typeof(Models.Translation), ("Translations", "Translations") },
        { typeof(Models.LabelNoteAssociation), ("LabelNoteAssociations", "LabelNoteAssociations") },
        { typeof(Models.Note), ("Replies", "Replies") },
        { typeof(Models.VerseRow), ("VerseRowsByBook", "VerseRows") },
        { typeof(NoteModelRef), ("NoteModelRefs", "NoteModelRefs") }
    };

    public static JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        IncludeFields = true,
        WriteIndented = true,
        TypeInfoResolver = new PolymorphicTypeResolver(),
        Converters = {
                    new JsonStringEnumConverter(),
                    new GeneralModelJsonConverter(),
                    new VerseRowModelListJsonConverter()
                }
    };

    public static JsonSerializerOptions JsonDeserializerOptions => new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        IncludeFields = true,
        //WriteIndented = true,
        TypeInfoResolver = new PolymorphicTypeResolver(),
        Converters = {
                    new JsonStringEnumConverter(),
                    new GeneralModelJsonConverter(),
                    new VerseRowModelListJsonConverter(),
                    new NoteModelRefJsonConverter()
                }
    };

    public static ProjectSnapshot BuildEmptySnapshot(Models.Project project)
    {
        return new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(project));
    }
}

