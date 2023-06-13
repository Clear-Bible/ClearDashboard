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

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Corpus>(), null);

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.TokenizedCorpus>(),
            (string parentPath,
            GeneralModel<Models.TokenizedCorpus> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.TokenizedCorpus, Models.TokenComposite>(parentPath, modelSnapshot, cancellationToken);
                SaveGeneralModelChild<Models.TokenizedCorpus, Models.VerseRow>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.ParallelCorpus>(),
            (string parentPath,
            GeneralModel<Models.ParallelCorpus> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.ParallelCorpus, Models.TokenComposite>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.AlignmentSet>(),
            (string parentPath,
            GeneralModel<Models.AlignmentSet> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.AlignmentSet, Models.Alignment>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.TranslationSet>(),
            (string parentPath,
            GeneralModel<Models.TranslationSet> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.TranslationSet, Models.Translation>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Note>(),
            (string parentPath,
            GeneralModel<Models.Note> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Note, Models.Note>(parentPath, modelSnapshot, cancellationToken);
                SaveGeneralModelChild<Models.Note, NoteModelRef>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.Label>(),
            (string parentPath,
            GeneralModel<Models.Label> modelSnapshot) =>
            {
                SaveGeneralModelChild<Models.Label, Models.LabelNoteAssociation>(parentPath, modelSnapshot, cancellationToken);
            });

        SaveTopLevelEntities(_path, projectSnapshot.GetGeneralModelList<Models.User>(), null);
    }

    private void SaveTopLevelEntities<T>(
        string parentPath,
        IEnumerable<GeneralModel<T>> topLevelEntities,
        SaveGeneralModelChildDelegate<T>? saveGeneralModelChildDelegate)
        where T : notnull
    {
        var topLevelEntityTypePath = Path.Combine(parentPath, topLevelEntityFolderNameMappings[typeof(T)]);
        Directory.CreateDirectory(topLevelEntityTypePath);

        foreach (var topLevelEntity in topLevelEntities)
        {
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

    private void SaveGeneralModelChild<P,C>(string parentPath, GeneralModel<P> modelSnapshot, CancellationToken cancellationToken)
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
                var childModelShapshots = (GeneralListModel<NoteModelRef>?)children!;
                SaveChildren<NoteModelRef>(childPath, childModelShapshots, cancellationToken);
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
            else
            {
                var childModelShapshots = (GeneralListModel<GeneralModel<C>>?)children!;
                SaveChildren(childPath, childModelShapshots, cancellationToken);
            }

        }
    }

    private void SaveChildren<T>(string childPath, IEnumerable<T> modelSnapshots, CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        foreach (var modelSnapshot in modelSnapshots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serializedModelSnapshot = JsonSerializer.Serialize(modelSnapshot, _jsonSerializerOptions);
            File.WriteAllText(Path.Combine(childPath, modelSnapshot.GetId()!.ToString()!), serializedModelSnapshot);
        }
    }
}

