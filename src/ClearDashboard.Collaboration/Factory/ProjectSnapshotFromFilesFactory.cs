﻿using System;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.Exceptions;

namespace ClearDashboard.Collaboration.Factory;

public class ProjectSnapshotFromFilesFactory
{
    private delegate void AddGeneralModelChildDelegate<T>(
        IEnumerable<string> entityItems,
        GeneralModel<T> modelSnapshot) where T : notnull;

    private readonly string _path;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonDeserializerOptions;

    private readonly Dictionary<Type, string> topLevelEntityFolderNameMappings = ProjectSnapshotFactoryCommon.topLevelEntityFolderNameMappings;
    private readonly Dictionary<Type, (string folderName, string childName)> childFolderNameMappings = ProjectSnapshotFactoryCommon.childFolderNameMappings;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">Full path of a specific project.  Likely ending in something like 'Project_aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'</param>
    /// <param name="logger"></param>
    public ProjectSnapshotFromFilesFactory(string path, ILogger logger)
	{
        _path = path;
        _logger = logger;

        _jsonDeserializerOptions = ProjectSnapshotFactoryCommon.JsonDeserializerOptions;
    }

    public static IEnumerable<(Guid projectId, string projectName)> FindProjectIdsNames(string repositoryPath)
    {
        var projectIdNames = new List<(Guid projectId, string projectName)>();

        var searchPattern = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, "*");
        foreach (var d in Directory.GetDirectories(repositoryPath, searchPattern))
        {
            var projectPropertiesFilePath = d + Path.DirectorySeparatorChar + ProjectSnapshotFactoryCommon.PROPERTIES_FILE;
            if (File.Exists(projectPropertiesFilePath))
            {
                var serializedProjectModelSnapshot = File.ReadAllText(projectPropertiesFilePath);
                var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
                    serializedProjectModelSnapshot,
                    ProjectSnapshotFactoryCommon.JsonDeserializerOptions);

                projectIdNames.Add(((Guid)projectModelSnapshot!.GetId(), (string)projectModelSnapshot[nameof(Models.Project.ProjectName)]!));
            }
        }

        return projectIdNames;
    }

    public GeneralModel<Models.Project> LoadProject()
    {
        var topLevelEntries = Directory.EnumerateFileSystemEntries(_path).OrderBy(n => n);
        var projectModelSnapshot = LoadProjectProperties(topLevelEntries);

        return projectModelSnapshot;
    }

    public IEnumerable<GeneralModel<Models.User>> LoadUsers()
    {
        var topLevelEntries = Directory.EnumerateFileSystemEntries(_path).OrderBy(n => n);

        var topLevelEntry = topLevelEntries
            .Where(s => Path.GetFileName(s).Equals(topLevelEntityFolderNameMappings[typeof(Models.User)]))
            .SingleOrDefault();

        if (topLevelEntry is null)
        {
            throw new SerializedDataException($"No '{topLevelEntityFolderNameMappings[typeof(Models.User)]}' entry found in top level project entries");
        }

        return LoadTopLevelEntities<Models.User>(topLevelEntry, null);
    }

    public ProjectSnapshot LoadSnapshot(CancellationToken cancellationToken = default)
    {
        var topLevelEntries = Directory.EnumerateFileSystemEntries(_path).OrderBy(n => n);

        var projectModelSnapshot = LoadProjectProperties(topLevelEntries);
        var projectSnapshot = new ProjectSnapshot(projectModelSnapshot);

        foreach (var topLevelEntry in topLevelEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // topLevelEntry:  first level under project:  "AlignmentSets", "Corpora", etc.
            var topLevelEntryName = Path.GetFileName(topLevelEntry);

            if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.AlignmentSet)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.AlignmentSet>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.AlignmentSet> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.AlignmentSet, Models.Alignment>(entityItems, modelSnapshot, cancellationToken);
                    }));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.Corpus)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Corpus>(topLevelEntry, null));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.Label)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Label>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.Label> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.Label, Models.LabelNoteAssociation>(entityItems, modelSnapshot, cancellationToken);
                    }));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.Note)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Note>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.Note> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.Note, Models.Note>(entityItems, modelSnapshot, cancellationToken);
                        AddGeneralModelChild<Models.Note, NoteModelRef>(entityItems, modelSnapshot, cancellationToken);
                    }));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.TokenizedCorpus)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TokenizedCorpus>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.TokenizedCorpus> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.TokenizedCorpus, Models.TokenComposite>(entityItems, modelSnapshot, cancellationToken);
                        AddGeneralModelChild<Models.TokenizedCorpus, Models.VerseRow>(entityItems, modelSnapshot, cancellationToken);
                    }));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.ParallelCorpus)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.ParallelCorpus>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.ParallelCorpus> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.ParallelCorpus, Models.TokenComposite>(entityItems, modelSnapshot, cancellationToken);
                    }));
            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.TranslationSet)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TranslationSet>(topLevelEntry,
                    (IEnumerable<string> entityItems,
                    GeneralModel<Models.TranslationSet> modelSnapshot) =>
                    {
                        AddGeneralModelChild<Models.TranslationSet, Models.Translation>(entityItems, modelSnapshot, cancellationToken);
                    }));

            }
            else if (topLevelEntryName == topLevelEntityFolderNameMappings[typeof(Models.User)])
            {
                projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.User>(topLevelEntry, null));
            }
        }

        return projectSnapshot;
    }

    private GeneralModel<Models.Project> LoadProjectProperties(IEnumerable<string> topLevelEntries)
    {
        var projectPropertiesEntry = topLevelEntries.Where(s => Path.GetFileName(s).Equals(ProjectSnapshotFactoryCommon.PROPERTIES_FILE)).Single();

        var serializedProjectModelSnapshot = File.ReadAllText(projectPropertiesEntry);

        var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
            serializedProjectModelSnapshot,
            _jsonDeserializerOptions);

        if (projectModelSnapshot is null)
        {
            throw new SerializedDataException($"Unable to deserialize type 'GeneralModel<Models.Project>' properties at path {projectPropertiesEntry}");
        }

        return projectModelSnapshot;
    }

    private IEnumerable<GeneralModel<T>> LoadTopLevelEntities<T>(
        string topLevelEntry,
        AddGeneralModelChildDelegate<T>? addGeneralModelChildDelegate)
        where T : notnull
    {
        var modelSnapshots = new List<GeneralModel<T>>();

        foreach (var entityEntry in Directory.EnumerateDirectories(topLevelEntry).OrderBy(n => n))
        {
            // Second level under project, containing specific items for a single entity id: 
            var entityItems = Directory.EnumerateFileSystemEntries(entityEntry).OrderBy(n => n);

            var modelSnapshot = LoadGeneralModelProperties<T>(entityItems);

            if (addGeneralModelChildDelegate is not null)
            {
                addGeneralModelChildDelegate(entityItems, modelSnapshot);
            }

            modelSnapshots.Add(modelSnapshot);
        }

        return modelSnapshots;
    }

    private GeneralModel<T> LoadGeneralModelProperties<T>(IEnumerable<string> entityEntries)
        where T : notnull
    {
        var propertiesEntry = entityEntries.Where(s => Path.GetFileName(s).Equals(ProjectSnapshotFactoryCommon.PROPERTIES_FILE)).Single();

        var serializedModelSnapshot = File.ReadAllText(propertiesEntry);

        var modelSnapshot = JsonSerializer.Deserialize<GeneralModel<T>>(
            serializedModelSnapshot,
            _jsonDeserializerOptions)!;

        if (modelSnapshot is null)
        {
            throw new SerializedDataException($"Unable to deserialize type 'GeneralModel<{typeof(T).Name}>' properties at path {propertiesEntry}");
        }

        return modelSnapshot;
    }

    private void AddGeneralModelChild<P,C>(IEnumerable<string> entityEntries, GeneralModel<P> modelSnapshot, CancellationToken cancellationToken)
        where P : notnull
        where C : notnull
    {
        var folderName = childFolderNameMappings[typeof(C)].folderName;

        var childEntityEntry = entityEntries.Where(s => Path.GetFileName(s).Equals(folderName)).SingleOrDefault();

        if (childEntityEntry is not null)
        {
            if (typeof(C).IsAssignableTo(typeof(NoteModelRef)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildren<NoteModelRef>(childEntityEntry, cancellationToken);
                modelSnapshot.AddChild<NoteModelRef>(childName, childModelShapshots);
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.VerseRow)))
            {
                var childModelShapshots = new GeneralListModel<GeneralModel<C>>();

                foreach (var item in Directory.GetFiles(childEntityEntry).OrderBy(n => n))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var serializedChildModelSnapshot = File.ReadAllText(item);

                    var childModelSnapshot = JsonSerializer.Deserialize<GeneralListModel<GeneralModel<C>>>(
                        serializedChildModelSnapshot,
                        _jsonDeserializerOptions)!;

                    if (childModelSnapshot is null)
                    {
                        throw new SerializedDataException($"Unable to deserialize type 'GeneralListModel<GeneralModel<{typeof(C).Name}>>' properties at path {item}");
                    }

                    childModelShapshots.AddRange(childModelSnapshot);
                }

                var childName = childFolderNameMappings[typeof(C)].childName;
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Alignment)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildrenByGroup<Models.Alignment, AlignmentGroup>(childEntityEntry, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Translation)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildrenByGroup<Models.Translation, TranslationGroup>(childEntityEntry, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildren<GeneralModel<C>>(childEntityEntry, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
        }
    }

    private GeneralListModel<GeneralModel<M>> LoadChildrenByGroup<M,G>(string childEntry, CancellationToken cancellationToken)
        where M:notnull
        where G:ModelGroup<M>
    {
        var childModelShapshots = new GeneralListModel<GeneralModel<M>>();

        foreach (var item in Directory.GetFiles(childEntry).OrderBy(n => n))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serializedChildModelSnapshot = File.ReadAllText(item);

            var childModelSnapshotGroup = JsonSerializer.Deserialize<G>(
                serializedChildModelSnapshot,
                _jsonDeserializerOptions)!;

            if (childModelSnapshotGroup is null)
            {
                throw new SerializedDataException($"Unable to deserialize type '{typeof(G).ShortDisplayName()}' properties at path {item}");
            }

            childModelShapshots.AddRange(childModelSnapshotGroup.Items);
        }

        return childModelShapshots;
    }

    private IEnumerable<T> LoadChildren<T>(string childEntry, CancellationToken cancellationToken)
        where T : notnull
    {
        var childModelShapshots = new GeneralListModel<T>();

        DirectoryInfo directory = new DirectoryInfo(childEntry);
        FileInfo[] files = directory.GetFiles();

        var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
            .Select(f => f.FullName)
            .OrderBy(n => n);

        foreach (var item in filtered)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serializedChildModelSnapshot = File.ReadAllText(item);

            var childModelSnapshot = JsonSerializer.Deserialize<T>(
                serializedChildModelSnapshot,
                _jsonDeserializerOptions)!;

            if (childModelSnapshot is null)
            {
                throw new SerializedDataException($"Unable to deserialize type '{typeof(T).ShortDisplayName()}' properties at path {item}");
            }

            childModelShapshots.Add(childModelSnapshot);
        }

        return childModelShapshots;
    }
}
