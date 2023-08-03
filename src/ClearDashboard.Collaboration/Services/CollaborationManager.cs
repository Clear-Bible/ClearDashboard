using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System.Diagnostics;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Services;

public class CollaborationManager
{

    #region Member Variables

    private readonly ILogger<CollaborationManager> _logger;
    private readonly IMediator _mediator;
    private readonly IUserProvider _userProvider;
    private readonly IProjectProvider _projectProvider;

    private readonly string _repositoryBasePath =
        FilePathTemplates.CollabBaseDirectory + Path.DirectorySeparatorChar + "Collaboration";

    private readonly string _backupsPath =
        FilePathTemplates.CollabBaseDirectory + Path.DirectorySeparatorChar + "Backups";

    private readonly string _dumpsPath = FilePathTemplates.CollabBaseDirectory + Path.DirectorySeparatorChar + "Dumps";

    private Models.CollaborationConfiguration _configuration;
    private readonly bool _logMergeOnly = false;
    private string _repositoryPath = "LocalOnly";

    #endregion //Member Variables


    #region Public Properties

    public const string BranchName = "master";
    public const string RemoteOrigin = "origin";
    public string RepositoryPath => _repositoryPath;


    private const string UserSecretsId = "b02febcf-d7fc-48e1-abb1-f03647ca553c";
    private static readonly string _secretsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", UserSecretsId);
    private const string SecretsFileName = "secrets.json";
    private static readonly string _secretsFilePath = Path.Combine(_secretsFolderPath, SecretsFileName);



    #endregion //Public Properties


    #region Constructor

    public CollaborationManager(
        ILogger<CollaborationManager> logger,
        IMediator mediator,
        IUserProvider userProvider,
        IProjectProvider projectProvider,
        Models.CollaborationConfiguration configuration)
    {
        _logger = logger;
        _mediator = mediator;
        _userProvider = userProvider;
        _projectProvider = projectProvider;

        _configuration = configuration;

        if (!string.IsNullOrEmpty(_configuration.RemoteUrl))
        {
            try
            {
                var configRemoteUri = new Uri(_configuration.RemoteUrl);
                var lastPartOfPath = Path.GetFileNameWithoutExtension(configRemoteUri.AbsolutePath);

                _repositoryPath = _repositoryBasePath + Path.DirectorySeparatorChar + lastPartOfPath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Invalid value in RemoteUrl configuration: '{_configuration.RemoteUrl}'");
            }
        }
    }

    #endregion //Constructor


    #region Methods

    public void SaveCollaborationLicense(Models.CollaborationConfiguration collaborationConfiguration)
    {
        var configuration = new Models.CollaborationConfiguration
        {
            Group = collaborationConfiguration.Group,
            RemoteEmail = collaborationConfiguration.RemoteEmail,
            RemotePersonalAccessToken = collaborationConfiguration.RemotePersonalAccessToken,
            RemotePersonalPassword = collaborationConfiguration.RemotePersonalPassword,
            RemoteUrl = "",
            RemoteUserName = collaborationConfiguration.RemoteUserName,
            UserId = collaborationConfiguration.UserId,
            NamespaceId = collaborationConfiguration.NamespaceId,
        };

        var gitCollaboration = new GitCollaboration
        {
            GitAccessToken = configuration
        };

        var jsonString = JsonSerializer.Serialize(gitCollaboration);
        try
        {
            if (Directory.Exists(_secretsFolderPath) == false)
            {
                Directory.CreateDirectory(_secretsFolderPath);
            }

            File.WriteAllText(_secretsFilePath, jsonString);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        

    }


    private Models.Project EnsureCurrentProject()
    {
        if (_projectProvider.CurrentProject is null)
        {
            // FIXME:  is there a more specific project-not-loaded exception somewhere?
            throw new Exception($"Collaboration commit error:  no project loaded");
        }

        if (_userProvider.CurrentUser is null)
        {
            // FIXME:  is there a more specific project-not-loaded exception somewhere?
            throw new Exception($"Collaboration commit error:  no current user");
        }

        return _projectProvider.CurrentProject!;
    }

    private static void EnsureValidRepository(string path)
    {
        if (!Repository.IsValid(path))
        {
            throw new GitRepositoryNotFoundException(path);
        }
    }

    private void EnsureValidRemoteUrl()
    {
        if (HasRemoteConfigured())
        {
            using (var repo = new Repository(_repositoryPath))
            {
                var gitRemoteUrl = repo.Config.Where(e => e.Key == "remote.origin.url").FirstOrDefault();
                if (gitRemoteUrl is not null)
                {
                    var gitRemoteUri = new Uri(gitRemoteUrl.Value);
                    var configRemoteUri = new Uri(_configuration.RemoteUrl);

                    if (gitRemoteUri.Host == configRemoteUri.Host ||
                        gitRemoteUri.AbsolutePath != configRemoteUri.AbsolutePath)
                    {
                        _logger.LogError(
                            $"Current git repository remote.origin.url '{gitRemoteUri}' is different than what is configuration in secrets.json '{configRemoteUri}'.  Either delete the Collaboration folder and start over or change the secrets.json file to match.");
                        throw new GitRepositoryNotFoundException(
                            $"Current git repository remote.origin.url '{gitRemoteUri}' is different than what is configuration in secrets.json '{configRemoteUri}'.  Either delete the Collaboration folder and start over or change the secrets.json file to match.");
                    }
                }
            }
        }
    }

    public bool HasRemoteConfigured()
    {
        return
            !string.IsNullOrEmpty(_configuration.RemoteEmail) &&
            !string.IsNullOrEmpty(_configuration.RemoteUserName) &&
            !string.IsNullOrEmpty(_configuration.RemotePersonalAccessToken);
    }

    public bool IsRepositoryInitialized()
    {
        return Repository.IsValid(_repositoryPath);
    }

    public void InitializeRepository()
    {
        if (!Repository.IsValid(_repositoryPath))
        {
            Repository.Init(_repositoryPath);
        }

        using (var repo = new Repository(_repositoryPath))
        {
            if (!repo.Network.Remotes.Any() && HasRemoteConfigured())
            {
                repo.Network.Remotes.Add(RemoteOrigin, _configuration.RemoteUrl);
                Remote remote = repo.Network.Remotes[RemoteOrigin];

                var refSpec = repo.Config.Get<string>("remote", remote.Name, "fetch");
            }
        }
    }

    public bool IsCurrentProjectInRepository()
    {
        if (IsRepositoryInitialized() && _projectProvider.HasCurrentProject)
        {
            var currentProjectId = _projectProvider.CurrentProject!.Id;
            return ProjectSnapshotFromGitFactory.IsProjectInRepository(currentProjectId, _repositoryPath);
        }

        return false;
    }

    public string GetRemoteSha(string? dashboardProjectFullFilePath, Guid dashboardProjectId)
    {
        var path =Path.Combine( _repositoryBasePath, "P_" + dashboardProjectId);

        if (Directory.Exists(path))
        {
            _repositoryPath = path;
            var result = FindRemoteHeadCommitSha();

            if (result != null)
            {
                return result;
            }
        }

        return string.Empty;
    }

    protected string? FindRemoteHeadCommitSha()
    {
        using (var repo = new Repository(_repositoryPath))
        {
            var credentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                new UsernamePasswordCredentials()
                {
                    Username = _configuration.RemoteUserName,
                    Password = _configuration.RemotePersonalAccessToken
                });

            foreach (Remote remote in repo.Network.Remotes)
            {
                if (remote.Name == RemoteOrigin)
                {
                    try
                    {
                        var references = repo.Network.ListReferences(remote, credentialsProvider);
                        var headRef = references.Where(e => e.CanonicalName == "HEAD").FirstOrDefault();
                        if (headRef is not null)
                        {
                            var headCommitSha = headRef.ResolveToDirectReference().TargetIdentifier;
                            return headCommitSha;
                        }
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
        }

        return null;
    }

    public bool AreUnmergedChanges()
    {
        if (_repositoryPath == "LocalOnly")
        {
            return false;
        }

        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        try
        {
            var remoteHeadCommitSha = FindRemoteHeadCommitSha();
            return project.LastMergedCommitSha != remoteHeadCommitSha;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(
                $"Unable to determine if there are any unmerged changes, so to be safe returning true.  Exception: {ex.Message}");
            return true;
        }
    }

    private static GeneralModel<Models.Project> LoadProjectModelSnapshot(string projectPropertiesFilePath)
    {
        var serializedProjectModelSnapshot = File.ReadAllText(projectPropertiesFilePath);

        var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
            serializedProjectModelSnapshot,
            ProjectSnapshotFactoryCommon.JsonDeserializerOptions);

        if (projectModelSnapshot is null)
        {
            throw new SerializedDataException($"Serialized project data invalid at path: {projectPropertiesFilePath}");
        }

        return projectModelSnapshot;
    }

    private IReadOnlyDictionary<Guid, GeneralModel<Models.Project>> GetAllProjectModelSnapshotsById()
    {
        var projects = new Dictionary<Guid, GeneralModel<Models.Project>>();
        foreach (var d in Directory.GetDirectories(RepositoryPath, "Project_*"))
        {
            var projectPropertiesFilePath = d + Path.DirectorySeparatorChar + "_Properties";
            if (File.Exists(projectPropertiesFilePath))
            {
                var projectModelSnapshot = LoadProjectModelSnapshot(projectPropertiesFilePath);
                projects.Add((Guid)projectModelSnapshot.GetId(), projectModelSnapshot);
            }
        }

        return projects;
    }

    public IEnumerable<(Guid projectId, string projectName, string appVersion, DateTimeOffset created)> GetAllProjects()
    {
        return GetAllProjectModelSnapshotsById().Select(kvp => (
            kvp.Key,
            (string)kvp.Value.PropertyValues[nameof(Models.Project.ProjectName)]!,
            (string)kvp.Value.PropertyValues[nameof(Models.Project.AppVersion)]!,
            (DateTimeOffset)kvp.Value.PropertyValues[nameof(Models.Project.Created)]!)
        );
    }

    public string? LoadIntoProjectProvider(Guid projectId)
    {
        var projectModelSnapshot = GetAllProjectModelSnapshotsById()
            .Where(e => e.Key == projectId)
            .Select(e => e.Value)
            .FirstOrDefault();

        if (projectModelSnapshot is null)
        {
            return null;
        }

        var project = ProjectBuilder.BuildModel(projectModelSnapshot);
        _projectProvider.CurrentProject = project;

        return project.ProjectName ?? "Unnamed-Project";
    }

    // TODO:  create repo-sticky file to put into project folder:
    public async Task<string> InitializeProjectDatabaseAsync(Guid projectId, bool includeMerge,
        CancellationToken cancellationToken, IProgress<ProgressStatus> progress)
    {
        using (var repo = new Repository(_repositoryPath))
        {
            var headCommitSha = repo.Commits.Select(c => c.Sha).FirstOrDefault();

            if (headCommitSha is not null)
            {
                var command =
                    new InitializeDatabaseCommand(_repositoryPath, headCommitSha, projectId, includeMerge, progress);
                var result = await _mediator.Send(command, cancellationToken);
                result.ThrowIfCanceledOrFailed();

                return headCommitSha;
            }
            else
            {
                throw new CommitNotFoundException(
                    $"No commits found for project Id '{projectId}' with which to initialize project database");
            }
        }
    }

    public void FetchMergeRemote()
    {
        EnsureValidRepository(_repositoryPath);

        string logMessage = "";
        using (var repo = new Repository(_repositoryPath))
        {
            FetchOptions options = new FetchOptions();
            options.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                new UsernamePasswordCredentials()
                {
                    Username = _configuration.RemoteUserName,
                    Password = _configuration.RemotePersonalAccessToken
                });

            foreach (Remote remote in repo.Network.Remotes)
            {
                IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                try
                {
                    Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                }
            }

            var signature = new LibGit2Sharp.Signature(
                new Identity(_configuration.RemoteUserName, _configuration.RemoteEmail), DateTimeOffset.Now);

            foreach (var b in repo.Branches)
            {
                repo.Merge(b.Tip, signature);
            }
        }

        _logger.LogInformation(logMessage);
    }

    public void PullRemoteCommits()
    {
        EnsureValidRepository(_repositoryPath);

        using (var repo = new Repository(_repositoryPath))
        {
            LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
            options.FetchOptions = new FetchOptions();
            options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = _configuration.RemoteUserName,
                        Password = _configuration.RemotePersonalAccessToken
                    });

            var signature = new LibGit2Sharp.Signature(
                new Identity(_configuration.RemoteUserName, _configuration.RemoteEmail), DateTimeOffset.Now);

            Commands.Pull(repo, signature, options);
        }
    }

    // TODO:  throw an exception if the contents of the project-sticky file
    // isn't there or doesn't match the current repository configuration
    public async Task<string?> MergeProjectLatestChangesAsync(MergeMode mergeMode, bool createBackupSnapshot,
        CancellationToken cancellationToken, IProgress<ProgressStatus> progress)
    {
        progress.Report(new ProgressStatus(0, "Finding latest commit"));

        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        var headCommitSha = string.Empty;
        var lastMergedCommitShaIndex = -1;

        using (var repo = new Repository(_repositoryPath))
        {
            // From latest to earliest:
            var commitShas = repo.Commits.Select(c => c.Sha).ToList();

            if (!commitShas.Any())
            {
                progress.Report(new ProgressStatus(0, $"Repository has no commits yet"));
                _logger.LogInformation($"MergeLastestChangesAsync called, but repository has no commits yet");
                return null;
            }

            headCommitSha = commitShas.First();
            lastMergedCommitShaIndex = commitShas.FindIndex(c => c == project.LastMergedCommitSha);
        }

        if (lastMergedCommitShaIndex == -1 && project.LastMergedCommitSha is not null)
        {
            // FIXME:  not sure what to do here.  Wrong commit tree or something?
            progress.Report(new ProgressStatus(0,
                $"Last merged commit sha '{project.LastMergedCommitSha}' is not in commit tree.  Unable to merge!"));
            _logger.LogInformation(
                $"Last merged commit sha '{project.LastMergedCommitSha}' is not in commit tree.  Unable to merge!");
            throw new CommitNotFoundException(project.LastMergedCommitSha);
        }

        if (lastMergedCommitShaIndex == 0)
        {
            // We are already at the latest
            progress.Report(new ProgressStatus(0, $"Already at latest commit '{project.LastMergedCommitSha}'"));
            _logger.LogInformation(
                $"MergeLastestChangesAsync called, but already at latest commit '{project.LastMergedCommitSha}'");
            return null;
        }

        var factory = new ProjectSnapshotFromGitFactory(_repositoryPath, _logger);

        progress.Report(new ProgressStatus(0, "Creating project snapshots for merge"));

        var projectSnapshotLastMerged = (lastMergedCommitShaIndex == -1)
            ? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(project.Id)
            : factory.LoadSnapshot(project.LastMergedCommitSha!, project.Id);
        cancellationToken.ThrowIfCancellationRequested();

        var projectSnapshotToMerge = factory.LoadSnapshot(headCommitSha, project.Id);
        cancellationToken.ThrowIfCancellationRequested();

        // Create a backup snapshot of the current database:
        if (createBackupSnapshot)
        {
            progress.Report(new ProgressStatus(0, "Creating backup snapshot of current database state"));
            await CreateProjectBackupAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Merge into the project database:
        var command = new MergeProjectSnapshotCommand(headCommitSha, projectSnapshotLastMerged, projectSnapshotToMerge,
            mergeMode, _logMergeOnly, progress);
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();

        if (!_logMergeOnly)
        {
            project.LastMergedCommitSha = headCommitSha;
            return headCommitSha;
        }

        return project.LastMergedCommitSha;
    }

    public async Task CreateProjectBackupAsync(CancellationToken cancellationToken)
    {
        var project = EnsureCurrentProject();

        Directory.CreateDirectory(_backupsPath);

        var folderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id) +
                         DateTimeOffset.UtcNow.ToString("__yyyy-MM-dd_HH-mm-ss");

        // Extract the latest from the project database:
        var command = new GetProjectSnapshotQuery();
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();

        // Save it to the local backup directory:
        var factory = new ProjectSnapshotFilesFactory(Path.Combine(_backupsPath, folderName), _logger);
        factory.SaveSnapshot(result.Data!);
    }

    // TODO:  throw an exception if the contents of the project-sticky file
    // isn't there or doesn't match the current repository configuration
    public async Task StageProjectChangesAsync(CancellationToken cancellationToken, IProgress<ProgressStatus> progress)
    {
        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id);

        progress.Report(new ProgressStatus(0, "Extracting project snapshot from database for staging"));

        // Extract the latest from the project database:
        var command = new GetProjectSnapshotQuery();
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();

        progress.Report(new ProgressStatus(0, "Saving project snapshot to source control directory"));

        // Save it to the local git directory:
        var factory = new ProjectSnapshotFilesFactory(Path.Combine(_repositoryPath, projectFolderName), _logger);
        factory.SaveSnapshot(result.Data!);

        // Stage the changes:  (or maybe have this as a separate step?)
        using (var repo = new Repository(_repositoryPath))
        {
            progress.Report(new ProgressStatus(0, "Staging changes in source control"));
            try
            {
                Commands.Stage(repo, projectFolderName);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }

    public void HardResetChanges()
    {
        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id);

        using (var repo = new Repository(_repositoryPath))
        {
            Branch originMaster = repo.Branches["origin/master"];
            if (originMaster is not null)
            {
                repo.Reset(ResetMode.Hard, originMaster.Tip);
            }
        }
    }

    public IEnumerable<string> RetrieveFileStatuses(Guid? projectIdFilter = default)
    {
        EnsureValidRepository(_repositoryPath);

        var statusOptions = new LibGit2Sharp.StatusOptions();
        if (projectIdFilter != default)
        {
            var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName((Guid)projectIdFilter!);
            statusOptions.PathSpec = new[] { $"{projectFolderName}{Path.DirectorySeparatorChar}" };
        }

        FileStatus[] fileStatuses =
        {
            LibGit2Sharp.FileStatus.NewInWorkdir,
            LibGit2Sharp.FileStatus.ModifiedInWorkdir,
            LibGit2Sharp.FileStatus.NewInIndex,
            LibGit2Sharp.FileStatus.ModifiedInIndex,
            LibGit2Sharp.FileStatus.RenamedInIndex,
            LibGit2Sharp.FileStatus.DeletedFromIndex
        };

        var statuses = new List<string>();

        using (var repo = new Repository(_repositoryPath))
        {
            foreach (var item in repo.RetrieveStatus(statusOptions))
            {
                if (fileStatuses.Where(fs => item.State.HasFlag(fs)).Any())
                {
                    // FIXME:  lame, but I have no idea what format
                    // might be useful to the caller...
                    statuses.Add($"{item.State}:  {item.FilePath}");
                }
            }
        }

        return statuses;
    }

    public string? CommitChanges(string commitMessage, IProgress<ProgressStatus> progress)
    {
        EnsureValidRepository(_repositoryPath);

        //GitHelper.RetrieveStatus(path, Logger, LibGit2Sharp.FileStatus.NewInIndex, LibGit2Sharp.FileStatus.ModifiedInIndex, LibGit2Sharp.FileStatus.RenamedInIndex, LibGit2Sharp.FileStatus.DeletedFromIndex);

        //_ = EnsureCurrentProject();

        var userSignature = new Signature(
            _configuration.RemoteUserName,
            _configuration.RemoteEmail,
            DateTimeOffset.UtcNow
        );

        using (var repo = new Repository(_repositoryPath))
        {
            RepositoryStatus status = repo.RetrieveStatus();

            // TODO WATCH THESE FOR ALL CASES
            if (status.IsDirty && (!IsCurrentProjectInRepository() || status.Staged.Any() || status.Added.Any() || status.Removed.Any()))
            {
                progress.Report(new ProgressStatus(0, "Committing changes to source control"));
                var commit = repo.Commit(commitMessage, userSignature, userSignature);
                return commit.Sha;
            }

            progress.Report(new ProgressStatus(0, "Nothing to commit"));
            return null;
        }
    }

    public void PushChangesToRemote()
    {
        EnsureValidRepository(_repositoryPath);

        using (var repo = new Repository(_repositoryPath))
        {
            Remote remote = repo.Network.Remotes[RemoteOrigin];
            var options = new PushOptions();
            options.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials
                { Username = _configuration.RemoteUserName, Password = _configuration.RemotePersonalAccessToken };
            repo.Network.Push(remote, @"refs/heads/master", options);
        }

    }

    /// <summary>
    /// Determine which projects in repository contributed changes to the
    /// given commit.  Like doing the following git command and determining
    /// which project(s) the affected files are in:
    /// git diff-tree --no-commit-id --name-only [commitSha] -r
    /// </summary>
    /// <param name="commitSha">If null, use HEAD</param>
    public IEnumerable<Guid> FindProjectIdsInCommit(string? commitSha = null)
    {
        var projectIds = new List<Guid>();

        using (var repo = new Repository(_repositoryPath))
        {
            // From latest to earliest:
            var commits = repo.Commits.ToList();

            var commitShaIndex = 0;
            if (commitSha is not null)
            {
                commitShaIndex = commits.Select(e => e.Sha).ToList().FindIndex(c => c.StartsWith(commitSha));
                if (commitShaIndex < 0)
                {
                    throw new ArgumentException($"CommitSha '{commitSha}' not found");
                }
            }

            var newTree = commits.ElementAtOrDefault(commitShaIndex)?.Tree;
            var oldTree = commits.ElementAtOrDefault(commitShaIndex + 1)?.Tree;

            var projectIdsInCommit = new List<Guid>();
            if (newTree is not null)
            {
                var projectPaths = Enumerable.Empty<string>();
                if (oldTree is not null)
                {
                    projectPaths = repo.Diff.Compare<TreeChanges>(oldTree, newTree)
                        .Select(e => e.Path).ToTopLevelPropertiesPaths();
                }
                else
                {
                    projectPaths = newTree
                        .Select(e => e.Path).ToTopLevelPropertiesPaths();
                }

                foreach (var path in projectPaths)
                {
                    var projectPropertiesEntry = repo.Lookup<Blob>($"{commitSha}:{path}");
                    var serializedProjectModelSnapshot = projectPropertiesEntry.GetContentText();

                    try
                    {
                        var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
                            serializedProjectModelSnapshot,
                            ProjectSnapshotFactoryCommon.JsonDeserializerOptions);

                        if (projectModelSnapshot is null)
                        {
                            throw new SerializedDataException($"Serialized project data invalid at path: {path}");
                        }

                        projectIds.Add((Guid)projectModelSnapshot.GetId());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }
            }
        }

        return projectIds;
    }

    public void DumpDifferencesBetweenLastMergedCommitAndHead()
    {
        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        using (var repo = new Repository(_repositoryPath))
        {
            var commitShas = repo.Commits.Select(c => c.Sha).ToList();

            if (!commitShas.Any())
            {
                _logger.LogInformation($"No commits - unable to dump differences between last merged commit and head");
                return;
            }

            if (project.LastMergedCommitSha is null)
            {
                _logger.LogInformation(
                    $"No last merged commit for project '{project.ProjectName}' - unable to dump differences between last merged commit and head");
                return;
            }

            var lastMergedCommitShaIndex = commitShas.FindIndex(c => c == project.LastMergedCommitSha);
            if (lastMergedCommitShaIndex == -1)
            {
                _logger.LogInformation(
                    $"Last merged commit for project '{project.ProjectName}' is not in git commit list - unable to dump differences between last merged commit and head");
                return;
            }

            var factory = new ProjectSnapshotFromGitFactory(_repositoryPath, _logger);

            var projectSnapshotHead = factory.LoadSnapshot(commitShas.First(), project.Id);
            var projectSnapshotLastMerged = factory.LoadSnapshot(project.LastMergedCommitSha, project.Id);

            var projectDifferences =
                new ProjectDifferences(projectSnapshotLastMerged, projectSnapshotHead, CancellationToken.None);

            var folderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id) + "_LastMergedToHead" +
                             DateTimeOffset.UtcNow.ToString("__yyyy-MM-dd_HH-mm-ss");
            projectDifferences.Serialize(Path.Combine(_dumpsPath, folderName));
        }
    }

    public async Task DumpDifferencesBetweenHeadAndCurrentDatabaseAsync()
    {
        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        using (var repo = new Repository(_repositoryPath))
        {
            var commitShas = repo.Commits.Select(c => c.Sha).ToList();

            if (!commitShas.Any())
            {
                _logger.LogInformation($"No commits - unable to dump differences between head and current database");
                return;
            }

            var factory = new ProjectSnapshotFromGitFactory(_repositoryPath, _logger);
            var projectSnapshotHead = factory.LoadSnapshot(commitShas.First(), project.Id);

            // Extract the latest from the project database:
            var command = new GetProjectSnapshotQuery();
            var result = await _mediator.Send(command, CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogInformation($"Unable to extract latest from project database: {result.Message}");
                return;
            }

            var projectDifferences = new ProjectDifferences(projectSnapshotHead, result.Data!, CancellationToken.None);

            var folderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id) + "_HeadToDb" +
                             DateTimeOffset.UtcNow.ToString("__yyyy-MM-dd_HH-mm-ss");
            projectDifferences.Serialize(Path.Combine(_dumpsPath, folderName));
        }
    }

    public Models.CollaborationConfiguration GetConfig()
    {
        return new Models.CollaborationConfiguration
        {
            Group = _configuration.Group,
            RemoteEmail = _configuration.RemoteEmail,
            RemotePersonalAccessToken = _configuration.RemotePersonalAccessToken,
            RemotePersonalPassword = _configuration.RemotePersonalPassword,
            RemoteUrl = _configuration.RemoteUrl,
            RemoteUserName = _configuration.RemoteUserName,
            UserId = _configuration.UserId,
            NamespaceId = _configuration.NamespaceId,
        };
    }

    public void SetRemoteUrl(string userInfoRemoteUrl, string guid)
    {
        userInfoRemoteUrl = userInfoRemoteUrl.Replace("http:", "https:");
        _configuration.RemoteUrl = userInfoRemoteUrl;
        _repositoryPath = Path.Combine(_repositoryBasePath,guid);
    }

    #endregion Methods
}


//internal static class RepositoryExtensions
//    {
//        internal static IEnumerable<string> ToTopLevelPropertiesPaths(this IEnumerable<string> paths)
//        {
//            return paths.Select(e => e.Split(Path.DirectorySeparatorChar)[0])
//                .Distinct()
//                .Select(e => $"{e}{Path.DirectorySeparatorChar}_Properties")
//                .ToList();
//        }

//        internal static IEnumerable<Guid> ToProjectIds(this IEnumerable<string> paths)
//        {
//            return paths
//                .Select(e => e.Split(Path.DirectorySeparatorChar)[0].Substring("Project_".Length))
//                .Distinct()
//                .Select(e => (success: Guid.TryParse(e, out var id), id: id))
//                .Where(parsed => parsed.success)
//                .Select(parsed => parsed.id)
//                .ToList();
//        }
//    }