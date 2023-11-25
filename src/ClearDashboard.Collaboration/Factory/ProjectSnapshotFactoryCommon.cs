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
    public static readonly string AlignmentsByLocationFileNameTemplate = $"Alignments_{{0}}_{{1}}";
    public static readonly string TranslationsByLocationFileNameTemplate = $"Translations_{{0}}_{{1}}";

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
        { typeof(Models.TranslationSet), "TranslationSets" },
        { typeof(Models.User), "Users" },
        { typeof(Models.Lexicon_Lexeme), "LexiconLexemes" },
        { typeof(Models.Lexicon_SemanticDomain), "LexiconSemanticDomains" }
    };

    public static readonly Dictionary<Type, (string folderName, string childName)> childFolderNameMappings = new() {
        { typeof(Models.Alignment), ("Alignments", "Alignments") },
        { typeof(Models.TokenComposite), ("CompositeTokens", "CompositeTokens") },
        { typeof(Models.Token), ("Tokens", "Tokens") },
        { typeof(Models.Translation), ("Translations", "Translations") },
        { typeof(Models.LabelNoteAssociation), ("LabelNoteAssociations", "LabelNoteAssociations") },
        { typeof(Models.Note), ("Replies", "Replies") },
        { typeof(Models.VerseRow), ("VerseRowsByBook", "VerseRows") },
        { typeof(NoteModelRef), ("NoteModelRefs", "NoteModelRefs") },
        { typeof(Models.NoteUserSeenAssociation), ("SeenBy", "NoteUserSeenAssociations") },
        { typeof(Models.Lexicon_Meaning), ("Meanings", "Meanings") },
        { typeof(Models.Lexicon_Translation), ("Translations", "Translations") },
        { typeof(Models.Lexicon_Form), ("Forms", "Forms") },
        { typeof(Models.Lexicon_SemanticDomainMeaningAssociation), ("SemanticDomainMeaningAssociations", "SemanticDomainMeaningAssociations") }
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
                    new VerseRowModelListJsonConverter(),
                    new AlignmentGroupJsonConverter(),
                    new TranslationGroupJsonConverter()
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
                    new NoteModelRefJsonConverter(),
                    new AlignmentGroupJsonConverter(),
                    new TranslationGroupJsonConverter()
                }
    };

    public static ProjectSnapshot BuildEmptySnapshot(Guid projectId)
    {
        var modelSnapshot = new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(new Models.Project() { Id = projectId }));

        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.User>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.Lexicon_Lexeme>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.Lexicon_SemanticDomain>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.Corpus>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.TokenizedCorpus>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.ParallelCorpus>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.AlignmentSet>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.TranslationSet>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.Note>>());
        modelSnapshot.AddGeneralModelList(Enumerable.Empty<GeneralModel<Models.Label>>());

        return modelSnapshot;
    }
}

