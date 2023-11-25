using System;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ClearDashboard.Collaboration.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.DataAccessLayer.Data;

namespace ClearDashboard.Collaboration.Factory;

public class ProjectSnapshotFilesFactory
{
    private delegate void SaveGeneralModelChildDelegate<T>(
        string parentPath,
        GeneralModel<T> modelSnapshot) where T : notnull;

    private readonly string _path;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly Dictionary<Type, string> topLevelEntityFolderNameMappings = ProjectSnapshotFactoryCommon.topLevelEntityFolderNameMappings;
    private readonly Dictionary<Type, (string folderName, string childName)> childFolderNameMappings = ProjectSnapshotFactoryCommon.childFolderNameMappings;

    public ProjectSnapshotFilesFactory(string path, ILogger logger)
	{
        _path = path;
        _logger = logger;

        _jsonSerializerOptions = ProjectSnapshotFactoryCommon.JsonSerializerOptions;
    }

    public ProjectSnapshotFilesFactory(string repositoryPath, Guid projectId, ILogger logger)
    {
        var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(projectId);
        _path = Path.Combine(repositoryPath, projectFolderName);

        _logger = logger;

        _jsonSerializerOptions = ProjectSnapshotFactoryCommon.JsonSerializerOptions;
    }

    public void SaveSnapshot(ProjectSnapshot projectSnapshot, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_path))
        {
            Directory.Delete(_path, true);
        }
        
        Directory.CreateDirectory(_path);

        var serializedProject = JsonSerializer.Serialize(projectSnapshot.GetGeneralModelProject(), _jsonSerializerOptions);
        File.WriteAllText(Path.Combine(_path, ProjectSnapshotFactoryCommon.PROPERTIES_FILE), serializedProject);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Corpus>(), null, cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.TokenizedCorpus>(),
            (string parentPath,
            GeneralModel<Models.TokenizedCorpus> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.TokenizedCorpus, Models.Token>(parentPath, modelSnapshot, null, cancellationToken);
                SaveGeneralModelChild<Models.TokenizedCorpus, Models.TokenComposite>(parentPath, modelSnapshot, null, cancellationToken);
                SaveGeneralModelChild<Models.TokenizedCorpus, Models.VerseRow>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.ParallelCorpus>(),
            (string parentPath,
            GeneralModel<Models.ParallelCorpus> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.ParallelCorpus, Models.TokenComposite>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.AlignmentSet>(),
            (string parentPath,
            GeneralModel<Models.AlignmentSet> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.AlignmentSet, Models.Alignment>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.TranslationSet>(),
            (string parentPath,
            GeneralModel<Models.TranslationSet> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.TranslationSet, Models.Translation>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Note>(),
            (string parentPath,
            GeneralModel<Models.Note> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Note, Models.Note>(parentPath, modelSnapshot, null, cancellationToken);
                SaveGeneralModelChild<Models.Note, NoteModelRef>(parentPath, modelSnapshot, null, cancellationToken);
                SaveGeneralModelChild<Models.Note, Models.NoteUserSeenAssociation>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Label>(),
            (string parentPath,
            GeneralModel<Models.Label> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Label, Models.LabelNoteAssociation>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.User>(), null, cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Lexicon_Lexeme>(), 
            (string parentPath,
            GeneralModel<Models.Lexicon_Lexeme> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Lexicon_Lexeme, Models.Lexicon_Meaning>(
                    parentPath, 
                    modelSnapshot, 
                    (string parentPath, GeneralModel<Models.Lexicon_Meaning> childModelSnapshot) =>
                    {
                        SaveGeneralModelChild<Models.Lexicon_Meaning, Models.Lexicon_Translation>(parentPath, childModelSnapshot, null, cancellationToken);
                    }, 
                    cancellationToken);
                SaveGeneralModelChild<Models.Lexicon_Lexeme, Models.Lexicon_Form>(parentPath, modelSnapshot, null, cancellationToken);
            },
            cancellationToken);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Lexicon_SemanticDomain>(),
            (string parentPath,
            GeneralModel<Models.Lexicon_SemanticDomain> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Lexicon_SemanticDomain, Models.Lexicon_SemanticDomainMeaningAssociation>(
                    parentPath,
                    modelSnapshot,
                    null,
                    cancellationToken);
            },
            cancellationToken);
    }

    private void SaveTopLevelEntities<T>(
        string parentPath,
        IEnumerable<GeneralModel<T>> topLevelEntities,
        SaveGeneralModelChildDelegate<T>? saveGeneralModelChildDelegate,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var topLevelEntityTypePath = Path.Combine(parentPath, topLevelEntityFolderNameMappings[typeof(T)]);
        Directory.CreateDirectory(topLevelEntityTypePath);

        foreach (var topLevelEntity in topLevelEntities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var topLevelEntityPath = Path.Combine(topLevelEntityTypePath, topLevelEntity.GetId()!.ToString()!);
            Directory.CreateDirectory(topLevelEntityPath);

            var serializedTopLevelEntity = JsonSerializer.Serialize(topLevelEntity, _jsonSerializerOptions);
            File.WriteAllText(Path.Combine(topLevelEntityPath, ProjectSnapshotFactoryCommon.PROPERTIES_FILE), serializedTopLevelEntity);

            if (saveGeneralModelChildDelegate is not null)
            {
                saveGeneralModelChildDelegate(topLevelEntityPath, topLevelEntity);
            }
        }
    }

    private void SaveGeneralModelChild<P,C>(
        string parentPath, 
        GeneralModel<P> modelSnapshot, 
        SaveGeneralModelChildDelegate<C>? saveGeneralModelChildDelegate, 
        CancellationToken cancellationToken)
        where P : notnull
        where C : notnull
    {
        var childName = childFolderNameMappings[typeof(C)].childName;

        if (modelSnapshot.TryGetChildValue(childName, out var children) && children!.Any())
        {
            var folderName = childFolderNameMappings[typeof(C)].folderName;

            var childPath = Path.Combine(parentPath, folderName);
            Directory.CreateDirectory(childPath);

            if (typeof(C).IsAssignableTo(typeof(NoteModelRef)))
            {
                var childModelSnapshots = (IEnumerable<NoteModelRef>)children!;
                SaveModelSnapshotChildren(childPath, childModelSnapshots, cancellationToken);
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.VerseRow)))
            {
                var childModelShapshots = (GeneralListModel<GeneralModel<Models.VerseRow>>?)children;
                if (childModelShapshots is not null && childModelShapshots.Any())
                {
                    VerseRowBuilder.SaveVerseRows(childModelShapshots, childPath, _jsonSerializerOptions, cancellationToken);
                }
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Alignment)))
            {
                var childModelShapshots = (GeneralListModel<GeneralModel<Models.Alignment>>?)children;
                if (childModelShapshots is not null && childModelShapshots.Any())
                {
                    AlignmentBuilder.SaveAlignments(modelSnapshot, childModelShapshots, childPath, _jsonSerializerOptions, cancellationToken);
                }
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Translation)))
            {
                var childModelShapshots = (GeneralListModel<GeneralModel<Models.Translation>>?)children;
                if (childModelShapshots is not null && childModelShapshots.Any())
                {
                    TranslationBuilder.SaveTranslations(modelSnapshot, childModelShapshots, childPath, _jsonSerializerOptions, cancellationToken);
                }
            }
            else if (children!.GetType().IsAssignableTo(typeof(IEnumerable<GeneralModel<C>>)))
            {
                var childModelShapshots = (IEnumerable<GeneralModel<C>>)children!;
                SaveGeneralModelChildren(childPath, childModelShapshots, saveGeneralModelChildDelegate, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"{nameof(ProjectSnapshotFilesFactory)}.{nameof(SaveGeneralModelChild)} unsupported children type {children?.GetType().ShortDisplayName()}");
            }
        }
    }

    private void SaveModelSnapshotChildren<T>(
        string childPath,
        IEnumerable<T> childModelSnapshots,
        CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        foreach (var childModelSnapshot in childModelSnapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serializedModelSnapshot = JsonSerializer.Serialize(childModelSnapshot, _jsonSerializerOptions);
            File.WriteAllText(Path.Combine(childPath, childModelSnapshot.GetId()!.ToString()!), serializedModelSnapshot);
        }
    }

    private void SaveGeneralModelChildren<T>(
        string childPath,
        IEnumerable<GeneralModel<T>> childModelSnapshots,
        SaveGeneralModelChildDelegate<T>? saveGeneralModelChildDelegate,
        CancellationToken cancellationToken)
        where T : notnull
    {
        foreach (var childModelSnapshot in childModelSnapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (saveGeneralModelChildDelegate is not null)
            {
                var childEntityPath = Path.Combine(childPath, childModelSnapshot.GetId()!.ToString()!);
                Directory.CreateDirectory(childEntityPath);

                var serializedModelSnapshot = JsonSerializer.Serialize(childModelSnapshot, _jsonSerializerOptions);
                File.WriteAllText(Path.Combine(childEntityPath, ProjectSnapshotFactoryCommon.PROPERTIES_FILE), serializedModelSnapshot);

                saveGeneralModelChildDelegate(childEntityPath, childModelSnapshot);
            }
            else
            {
                var serializedModelSnapshot = JsonSerializer.Serialize(childModelSnapshot, _jsonSerializerOptions);
                File.WriteAllText(Path.Combine(childPath, childModelSnapshot.GetId()!.ToString()!), serializedModelSnapshot);
            }
        }
    }
}

