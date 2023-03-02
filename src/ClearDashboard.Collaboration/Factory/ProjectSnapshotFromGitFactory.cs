using System;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using Models = ClearDashboard.DataAccessLayer.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.Collaboration.Exceptions;
using Paratext.PluginInterfaces;
using SIL.Extensions;

namespace ClearDashboard.Collaboration.Factory;

public class ProjectSnapshotFromGitFactory
{
    private delegate void AddGeneralModelChildDelegate<T>(
        IEnumerable<TreeEntry> entityItems,
        GeneralModel<T> modelSnapshot,
        Repository repo,
        string commitSha) where T : notnull;

    private readonly string _repositoryPath;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonDeserializerOptions;

    private readonly Dictionary<Type, string> topLevelEntityFolderNameMappings = ProjectSnapshotFactoryCommon.topLevelEntityFolderNameMappings;
    private readonly Dictionary<Type, (string folderName, string childName)> childFolderNameMappings = ProjectSnapshotFactoryCommon.childFolderNameMappings;

    public ProjectSnapshotFromGitFactory(string repositoryPath, ILogger logger)
    {
        _repositoryPath = repositoryPath;
        _logger = logger;

        _jsonDeserializerOptions = ProjectSnapshotFactoryCommon.JsonDeserializerOptions;
    }

    public ProjectSnapshot LoadSnapshot(string commitSha, Guid projectId)
	{
        using (var repo = new Repository(_repositoryPath))
        {
            var commit = repo.Commits.Where(c => c.Id.StartsWith(commitSha)).SingleOrDefault();
            if (commit is null)
            {
                throw new CommitNotFoundException(commitSha);
            }

            var projectFolderName = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, projectId);

            var topLevelEntries = repo.Lookup<Tree>($"{commitSha}:{projectFolderName}");
            if (topLevelEntries is null)
            {
                throw new CommitObjectNotFoundException($"{commitSha}:{projectFolderName}");
            }

            var projectSnapshot = LoadProjectIntoSnapshot(topLevelEntries);

            foreach (var topLevelEntry in topLevelEntries.Where(te => te.TargetType == TreeEntryTargetType.Tree).OrderBy(te => te.Name))
            {
                // topLevelEntry:  first level under project:  "AlignmentSets", "Corpora", etc.

                if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.AlignmentSet)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.AlignmentSet>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.AlignmentSet> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.AlignmentSet, Models.Alignment>(entityItems, modelSnapshot, repo, commitSha);
                        }));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Corpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Corpus>(topLevelEntry, repo, commitSha, null));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Label)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Label>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Label> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Label, Models.LabelNoteAssociation>(entityItems, modelSnapshot, repo, commitSha);
                        }));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Note)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Note>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Note> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Note, Models.Note>(entityItems, modelSnapshot, repo, commitSha);
                            AddGeneralModelChild<Models.Note, NoteModelRef>(entityItems, modelSnapshot, repo, commitSha);
                        }));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.TokenizedCorpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TokenizedCorpus>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.TokenizedCorpus> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.TokenizedCorpus, Models.TokenComposite>(entityItems, modelSnapshot, repo, commitSha);
                            AddGeneralModelChild<Models.TokenizedCorpus, Models.VerseRow>(entityItems, modelSnapshot, repo, commitSha);
                        }));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.ParallelCorpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.ParallelCorpus>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.ParallelCorpus> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.ParallelCorpus, Models.TokenComposite>(entityItems, modelSnapshot, repo, commitSha);
                        }));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.TranslationSet)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TranslationSet>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.TranslationSet> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.TranslationSet, Models.Translation>(entityItems, modelSnapshot, repo, commitSha);
                        }));

                }
            }

            return projectSnapshot;
        }
    }

    private ProjectSnapshot LoadProjectIntoSnapshot(IEnumerable<TreeEntry> topLevelEntries)
    {
        var projectPropertiesEntry = topLevelEntries
            .Where(te => te.TargetType == TreeEntryTargetType.Blob && te.Name == ProjectSnapshotFactoryCommon.PROPERTIES_FILE)
            .Single();

        var serializedProjectModelSnapshot = ((Blob)projectPropertiesEntry.Target).GetContentText();

        var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
            serializedProjectModelSnapshot,
            _jsonDeserializerOptions);

        if (projectModelSnapshot is null)
        {
            throw new SerializedDataException($"Unable to deserialize type 'GeneralModel<Models.Project>' properties at path {projectPropertiesEntry.Path}");
        }

        var projectSnapshot = new ProjectSnapshot(projectModelSnapshot!);
        return projectSnapshot;
    }

    private IEnumerable<GeneralModel<T>> LoadTopLevelEntities<T>(
        TreeEntry topLevelEntry,
        Repository repo,
        string commitSha,
        AddGeneralModelChildDelegate<T>? addGeneralModelChildDelegate)
        where T : notnull
    {
        var modelSnapshots = new List<GeneralModel<T>>();

        var entityEntries = repo.Lookup<Tree>($"{commitSha}:{topLevelEntry.Path}").OrderBy(t => t.Name);

        foreach (var entityEntry in entityEntries.Where(te => te.TargetType == TreeEntryTargetType.Tree))
        {
            // Second level under project, containing specific items for a single entity id: 
            var entityItems = repo.Lookup<Tree>($"{commitSha}:{entityEntry.Path}");
            if (entityItems is null)
            {
                throw new CommitObjectNotFoundException($"{commitSha}:{entityEntry.Path}");
            }

            var modelSnapshot = LoadGeneralModelProperties<T>(entityItems);

            if (addGeneralModelChildDelegate is not null)
            {
                addGeneralModelChildDelegate(entityItems, modelSnapshot, repo, commitSha);
            }

            modelSnapshots.Add(modelSnapshot);
        }

        return modelSnapshots;
    }

    private GeneralModel<T> LoadGeneralModelProperties<T>(IEnumerable<TreeEntry> entityEntries)
        where T: notnull
    {
        var propertiesEntry = entityEntries
            .Where(te => te.TargetType == TreeEntryTargetType.Blob && te.Name == ProjectSnapshotFactoryCommon.PROPERTIES_FILE)
            .Single();

        var serializedModelSnapshot = ((Blob)propertiesEntry.Target).GetContentText();

        var modelSnapshot = JsonSerializer.Deserialize<GeneralModel<T>>(
            serializedModelSnapshot,
            _jsonDeserializerOptions)!;

        if (modelSnapshot is null)
        {
            throw new SerializedDataException($"Unable to deserialize type 'GeneralModel<{typeof(T).Name}>' properties at path {propertiesEntry.Path}");
        }

        return modelSnapshot;
    }

    private void AddGeneralModelChild<P,C>(IEnumerable<TreeEntry> entityEntries, GeneralModel<P> modelSnapshot, Repository repo, string commitSha)
        where P : notnull
        where C : notnull
    {
        var folderName = childFolderNameMappings[typeof(C)].folderName;

        var childEntityEntry = entityEntries
            .Where(te => te.TargetType == TreeEntryTargetType.Tree && te.Name == folderName)
            .OrderBy(te => te.Name)
            .FirstOrDefault();

        if (childEntityEntry is not null)
        {
            if (typeof(C).IsAssignableTo(typeof(NoteModelRef)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildren<NoteModelRef>(childEntityEntry, repo, commitSha);
                modelSnapshot.AddChild<NoteModelRef>(childName, childModelShapshots);
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.VerseRow)))
            {
                var childModelShapshots = new GeneralListModel<GeneralModel<C>>();

                var items = repo.Lookup<Tree>($"{commitSha}:{childEntityEntry.Path}");
                if (items is null)
                {
                    throw new CommitObjectNotFoundException($"{commitSha}:{childEntityEntry.Path}");
                }

                foreach (var item in items.Where(te => te.TargetType == TreeEntryTargetType.Blob).OrderBy(te => te.Name))
                {
                    var serializedChildModelSnapshot = ((Blob)item.Target).GetContentText();

                    var childModelSnapshot = JsonSerializer.Deserialize<GeneralListModel<GeneralModel<C>>>(
                        serializedChildModelSnapshot,
                        _jsonDeserializerOptions)!;

                    if (childModelSnapshot is null)
                    {
                        throw new SerializedDataException($"Unable to deserialize type 'GeneralListModel<GeneralModel<{typeof(C).Name}>>' properties at path {item.Path}");
                    }

                    childModelShapshots.AddRange(childModelSnapshot);
                }

                var childName = childFolderNameMappings[typeof(C)].childName;
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Alignment)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildrenByGroup<Models.Alignment, AlignmentGroup>(childEntityEntry, repo, commitSha);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Translation)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildrenByGroup<Models.Translation, TranslationGroup>(childEntityEntry, repo, commitSha);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildren<GeneralModel<C>>(childEntityEntry, repo, commitSha);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
        }
    }

    private GeneralListModel<GeneralModel<M>> LoadChildrenByGroup<M, G>(TreeEntry childEntry, Repository repo, string commitSha)
        where M : notnull
        where G : ModelGroup<M>
    {
        var childModelShapshots = new GeneralListModel<GeneralModel<M>>();

        var items = repo.Lookup<Tree>($"{commitSha}:{childEntry.Path}");
        if (items is null)
        {
            throw new CommitObjectNotFoundException($"{commitSha}:{childEntry.Path}");
        }

        foreach (var item in items.Where(te => te.TargetType == TreeEntryTargetType.Blob).OrderBy(te => te.Name))
        {
            var serializedChildModelSnapshot = ((Blob)item.Target).GetContentText();

            var childModelSnapshotGroup = JsonSerializer.Deserialize<G>(
                serializedChildModelSnapshot,
                _jsonDeserializerOptions)!;

            if (childModelSnapshotGroup is null)
            {
                throw new SerializedDataException($"Unable to deserialize type 'AlignmentGroup' properties at path {item.Path}");
            }

            childModelShapshots.AddRange(childModelSnapshotGroup.Items);
        }

        return childModelShapshots;
    }

    private IEnumerable<T> LoadChildren<T>(TreeEntry childEntry, Repository repo, string commitSha)
        where T : notnull
    {
        var childModelShapshots = new GeneralListModel<T>();

        var items = repo.Lookup<Tree>($"{commitSha}:{childEntry.Path}");
        if (items is null)
        {
            throw new CommitObjectNotFoundException($"{commitSha}:{childEntry.Path}");
        }

        foreach (var item in items.Where(te => te.TargetType == TreeEntryTargetType.Blob).OrderBy(te => te.Name))
        {
            var serializedChildModelSnapshot = ((Blob)item.Target).GetContentText();

            var childModelSnapshot = JsonSerializer.Deserialize<T>(
                serializedChildModelSnapshot,
                _jsonDeserializerOptions)!;

            if (childModelSnapshot is null)
            {
                throw new SerializedDataException($"Unable to deserialize type '{typeof(T).ShortDisplayName()}' properties at path {item.Path}");
            }

            childModelShapshots.Add(childModelSnapshot);
        }

        return childModelShapshots;
    }
}

