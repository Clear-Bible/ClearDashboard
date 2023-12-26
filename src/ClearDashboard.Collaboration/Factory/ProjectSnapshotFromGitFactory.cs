using System;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.DataAccessLayer.Models;

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

    public static bool IsProjectInRepository(Guid projectId, string repositoryPath)
    {
        if (!Repository.IsValid(repositoryPath))
            return false;

        using (var repo = new Repository(repositoryPath))
        {
            var commit = repo.Commits.FirstOrDefault();
            if (commit is null)
            {
                return false;
            }

            var projectFolderName = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, projectId);

            var topLevelEntries = repo.Lookup<Tree>($"{commit.Sha}:{projectFolderName}");
            if (topLevelEntries is null)
            {
                return false;
            }

            return true;
        }
    }

    public GeneralModel<Models.Project> LoadProject(string commitSha, string projectFolderName)
    {
        using (var repo = new Repository(_repositoryPath))
        {
            var topLevelEntries = Initialize(commitSha, projectFolderName, repo);
            var projectModelSnapshot = LoadProjectProperties(topLevelEntries);

            return projectModelSnapshot;
        }
    }

    public GeneralModel<Models.Project> LoadProject(string commitSha, Guid projectId)
    {
        var projectFolderName = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, projectId);
        return LoadProject(commitSha, projectFolderName);
    }

    public IEnumerable<GeneralModel<Models.User>> LoadUsers(string commitSha, Guid projectId, CancellationToken cancellationToken)
    {
        using (var repo = new Repository(_repositoryPath))
        {
            var projectFolderName = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, projectId);
            var topLevelEntries = Initialize(commitSha, projectFolderName, repo);

            var topLevelEntry = topLevelEntries
                .Where(te => te.TargetType == TreeEntryTargetType.Tree)
                .Where(te => te.Name == topLevelEntityFolderNameMappings[typeof(Models.User)])
                .SingleOrDefault();

            if (topLevelEntry is null)
            {
                throw new SerializedDataException($"No '{topLevelEntityFolderNameMappings[typeof(Models.User)]}' entry found in top level project entries");
            }

            return LoadTopLevelEntities<Models.User>(topLevelEntry, repo, commitSha, null, cancellationToken);
        }
    }

    public ProjectSnapshot LoadSnapshot(string commitSha, Guid projectId, CancellationToken cancellationToken = default)
	{
        using (var repo = new Repository(_repositoryPath))
        {
            var projectFolderName = string.Format(ProjectSnapshotFactoryCommon.ProjectFolderNameTemplate, projectId);

            var topLevelEntries = Initialize(commitSha, projectFolderName, repo);
            var projectModelSnapshot = LoadProjectProperties(topLevelEntries);

            var projectSnapshot = new ProjectSnapshot(projectModelSnapshot);

            foreach (var topLevelEntry in topLevelEntries.Where(te => te.TargetType == TreeEntryTargetType.Tree).OrderBy(te => te.Name))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // topLevelEntry:  first level under project:  "AlignmentSets", "Corpora", etc.

                if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.AlignmentSet)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.AlignmentSet>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.AlignmentSet> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.AlignmentSet, Models.Alignment>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        }, 
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Corpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Corpus>(topLevelEntry, repo, commitSha, null, cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.LabelGroup)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.LabelGroup>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.LabelGroup> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.LabelGroup, Models.LabelGroupAssociation>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Label)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Label>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Label> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Label, Models.LabelNoteAssociation>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Note)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Note>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Note> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Note, Models.Note>(
                                entityItems, 
                                modelSnapshot, 
                                repo, 
                                commitSha,
                                (IEnumerable<TreeEntry> childEntityItems,
                                GeneralModel<Models.Note> childModelSnapshot,
                                Repository repo,
                                string commitSha) =>
                                {
                                    AddGeneralModelChild<Models.Note, NoteModelRef>(childEntityItems, childModelSnapshot, repo, commitSha, null, cancellationToken);
                                    AddGeneralModelChild<Models.Note, Models.NoteUserSeenAssociation>(childEntityItems, childModelSnapshot, repo, commitSha, null, cancellationToken);
                                },
                                cancellationToken);
                            AddGeneralModelChild<Models.Note, NoteModelRef>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                            AddGeneralModelChild<Models.Note, Models.NoteUserSeenAssociation>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.TokenizedCorpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TokenizedCorpus>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.TokenizedCorpus> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.TokenizedCorpus, Models.Token>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                            AddGeneralModelChild<Models.TokenizedCorpus, Models.TokenComposite>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                            AddGeneralModelChild<Models.TokenizedCorpus, Models.VerseRow>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.ParallelCorpus)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.ParallelCorpus>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.ParallelCorpus> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.ParallelCorpus, Models.TokenComposite>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.TranslationSet)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.TranslationSet>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.TranslationSet> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.TranslationSet, Models.Translation>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));

                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.User)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.User>(topLevelEntry, repo, commitSha, null, cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Lexicon_Lexeme)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Lexicon_Lexeme>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Lexicon_Lexeme> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Lexicon_Lexeme, Models.Lexicon_Meaning>(
                                entityItems, 
                                modelSnapshot, 
                                repo, 
                                commitSha,
                                (IEnumerable<TreeEntry> childEntityItems,
                                GeneralModel<Models.Lexicon_Meaning> childModelSnapshot,
                                Repository repo,
                                string commitSha) =>
                                {
                                    AddGeneralModelChild<Models.Lexicon_Meaning, Models.Lexicon_Translation>(childEntityItems, childModelSnapshot, repo, commitSha, null, cancellationToken);
                                }, 
                                cancellationToken);
                            AddGeneralModelChild<Models.Lexicon_Lexeme, Models.Lexicon_Form>(entityItems, modelSnapshot, repo, commitSha, null, cancellationToken);
                        },
                        cancellationToken));
                }
                else if (topLevelEntry.Name == topLevelEntityFolderNameMappings[typeof(Models.Lexicon_SemanticDomain)])
                {
                    projectSnapshot.AddGeneralModelList(LoadTopLevelEntities<Models.Lexicon_SemanticDomain>(topLevelEntry, repo, commitSha,
                        (IEnumerable<TreeEntry> entityItems,
                        GeneralModel<Models.Lexicon_SemanticDomain> modelSnapshot,
                        Repository repo,
                        string commitSha) =>
                        {
                            AddGeneralModelChild<Models.Lexicon_SemanticDomain, Models.Lexicon_SemanticDomainMeaningAssociation>(
                                entityItems,
                                modelSnapshot,
                                repo,
                                commitSha,
                                null,
                                cancellationToken);
                        },
                        cancellationToken));
                }
            }

            // Any top level entity types for which there may be existing
            // serializations in older formats that need to be updated:
            var updateMappings = new Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>>();

            GeneralModelBuilder.GetModelBuilder<Models.AlignmentSet>().UpdateModelSnapshotFormat(projectSnapshot, updateMappings);
            GeneralModelBuilder.GetModelBuilder<Models.TranslationSet>().UpdateModelSnapshotFormat(projectSnapshot, updateMappings);
            GeneralModelBuilder.GetModelBuilder<Models.Label>().UpdateModelSnapshotFormat(projectSnapshot, updateMappings);
            GeneralModelBuilder.GetModelBuilder<Models.Lexicon_Lexeme>().UpdateModelSnapshotFormat(projectSnapshot, updateMappings);
            GeneralModelBuilder.GetModelBuilder<Models.Lexicon_SemanticDomain>().UpdateModelSnapshotFormat(projectSnapshot, updateMappings);

            return projectSnapshot;
        }
    }

    private static Tree Initialize(string commitSha, string projectFolderName, Repository repo)
    {
        var commit = repo.Commits.Where(c => c.Id.StartsWith(commitSha)).SingleOrDefault();
        if (commit is null)
        {
            throw new CommitNotFoundException(commitSha);
        }

        var topLevelEntries = repo.Lookup<Tree>($"{commitSha}:{projectFolderName}");
        if (topLevelEntries is null)
        {
            throw new CommitObjectNotFoundException($"{commitSha}:{projectFolderName}");
        }

        return topLevelEntries;
    }

    private GeneralModel<Models.Project> LoadProjectProperties(IEnumerable<TreeEntry> topLevelEntries)
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

        return projectModelSnapshot;
    }

    private IEnumerable<GeneralModel<T>> LoadTopLevelEntities<T>(
        TreeEntry topLevelEntry,
        Repository repo,
        string commitSha,
        AddGeneralModelChildDelegate<T>? addGeneralModelChildDelegate,
        CancellationToken cancellationToken)
        where T : IdentifiableEntity
    {
        var modelSnapshots = new List<GeneralModel<T>>();

        var entityEntries = repo.Lookup<Tree>($"{commitSha}:{topLevelEntry.Path}").OrderBy(t => t.Name);

        foreach (var entityEntry in entityEntries.Where(te => te.TargetType == TreeEntryTargetType.Tree))
        {
            cancellationToken.ThrowIfCancellationRequested();

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

    private void AddGeneralModelChild<P,C>(
        IEnumerable<TreeEntry> entityEntries, 
        GeneralModel<P> modelSnapshot, 
        Repository repo, 
        string commitSha,
        AddGeneralModelChildDelegate<C>? addGeneralModelChildDelegate,
        CancellationToken cancellationToken)
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
                var childModelShapshots = LoadChildren<NoteModelRef>(childEntityEntry, repo, commitSha, cancellationToken);
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
                    cancellationToken.ThrowIfCancellationRequested();

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
                var childModelShapshots = LoadChildrenByGroup<Models.Alignment, AlignmentGroup>(childEntityEntry, repo, commitSha, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else if (typeof(C).IsAssignableTo(typeof(Models.Translation)))
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadChildrenByGroup<Models.Translation, TranslationGroup>(childEntityEntry, repo, commitSha, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
            else
            {
                var childName = childFolderNameMappings[typeof(C)].childName;
                var childModelShapshots = LoadGeneralModelChildren(childEntityEntry, repo, commitSha, addGeneralModelChildDelegate, cancellationToken);
                modelSnapshot.AddChild(childName, childModelShapshots.AsModelSnapshotChildrenList());
            }
        }
    }

    private GeneralListModel<GeneralModel<M>> LoadChildrenByGroup<M, G>(TreeEntry childEntry, Repository repo, string commitSha, CancellationToken cancellationToken)
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
            cancellationToken.ThrowIfCancellationRequested();

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

    private IEnumerable<GeneralModel<T>> LoadGeneralModelChildren<T>(
        TreeEntry childEntry, 
        Repository repo, 
        string commitSha,
        AddGeneralModelChildDelegate<T>? addGeneralModelChildDelegate,
        CancellationToken cancellationToken)
        where T : notnull
    {
        var childModelShapshots = new GeneralListModel<GeneralModel<T>>();

        if (addGeneralModelChildDelegate is not null)
        {
            var entityEntries = repo.Lookup<Tree>($"{commitSha}:{childEntry.Path}").OrderBy(t => t.Name);

            foreach (var entityEntry in entityEntries.Where(te => te.TargetType == TreeEntryTargetType.Tree))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entityItems = repo.Lookup<Tree>($"{commitSha}:{entityEntry.Path}");
                if (entityItems is null)
                {
                    throw new CommitObjectNotFoundException($"{commitSha}:{entityEntry.Path}");
                }

                var childModelSnapshot = LoadGeneralModelProperties<T>(entityItems);

                addGeneralModelChildDelegate(entityItems, childModelSnapshot, repo, commitSha);

                childModelShapshots.Add(childModelSnapshot);
            }
        }
        else
        {
            var items = repo.Lookup<Tree>($"{commitSha}:{childEntry.Path}");
            if (items is null)
            {
                throw new CommitObjectNotFoundException($"{commitSha}:{childEntry.Path}");
            }

            foreach (var item in items.Where(te => te.TargetType == TreeEntryTargetType.Blob).OrderBy(te => te.Name))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serializedChildModelSnapshot = ((Blob)item.Target).GetContentText();

                var childModelSnapshot = JsonSerializer.Deserialize<GeneralModel<T>>(
                    serializedChildModelSnapshot,
                    _jsonDeserializerOptions)!;

                if (childModelSnapshot is null)
                {
                    throw new SerializedDataException($"Unable to deserialize type '{typeof(T).ShortDisplayName()}' properties at path {item.Path}");
                }

                childModelShapshots.Add(childModelSnapshot);
            }
        }

        return childModelShapshots;
    }

    private IEnumerable<T> LoadChildren<T>(TreeEntry childEntry, Repository repo, string commitSha, CancellationToken cancellationToken)
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
            cancellationToken.ThrowIfCancellationRequested();

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

