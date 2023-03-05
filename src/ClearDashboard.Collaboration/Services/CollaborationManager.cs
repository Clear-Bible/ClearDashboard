using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Merge;
using System.Configuration;
using System.Security.Policy;
using Microsoft.VisualBasic;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Model;
using System.Text.Json;
using Paratext.PluginInterfaces;

namespace ClearDashboard.Collaboration.Services;

public class CollaborationManager
{
    private readonly ILogger<CollaborationManager> _logger;
    private readonly IMediator _mediator;
    private readonly IUserProvider _userProvider;
    private readonly IProjectProvider _projectProvider;
    private readonly string _repositoryPath = FilePathTemplates.ProjectBaseDirectory + Path.DirectorySeparatorChar + "Collaboration";
    private readonly string _backupsPath = FilePathTemplates.ProjectBaseDirectory + Path.DirectorySeparatorChar + "Backups";

    private readonly CollaborationConfiguration _configuration;
    private readonly bool _logMergeOnly = false;

    public const string RemoteOrigin = "origin";

    public CollaborationManager(
		ILogger<CollaborationManager> logger,
		IMediator mediator,
        IUserProvider userProvider,
		IProjectProvider projectProvider,
        CollaborationConfiguration configuration)
	{
		_logger = logger;
		_mediator = mediator;
		_userProvider = userProvider;
		_projectProvider = projectProvider;

        _configuration = configuration;
    }

    public string RepositoryPath => _repositoryPath;

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

    private void EnsureValidRepository(string path)
    {
        if (!Repository.IsValid(path))
        {
            throw new GitRepositoryNotFoundException(path);
        }
    }

    public void InitializeRepository()
    {
        if (!Repository.IsValid(_repositoryPath))
        {
            Repository.Init(_repositoryPath);

            using (var repo = new Repository(_repositoryPath))
            {
                repo.Network.Remotes.Add(RemoteOrigin, _configuration.RemoteUrl);
                Remote remote = repo.Network.Remotes[RemoteOrigin];

                var refSpec = repo.Config.Get<string>("remote", remote.Name, "fetch");
            }
        }
    }

    private IReadOnlyDictionary<Guid, GeneralModel<Models.Project>> GetAllProjectModelSnapshotsById()
    {
        var projects = new Dictionary<Guid, GeneralModel<Models.Project>>();
        foreach (var d in Directory.GetDirectories(RepositoryPath, "Project_*"))
        {
            var projectPropertiesFilePath = d + Path.DirectorySeparatorChar + "_Properties";
            if (File.Exists(projectPropertiesFilePath))
            {
                var serializedProjectModelSnapshot = File.ReadAllText(projectPropertiesFilePath);

                var projectModelSnapshot = JsonSerializer.Deserialize<GeneralModel<Models.Project>>(
                    serializedProjectModelSnapshot,
                    ProjectSnapshotFactoryCommon.JsonDeserializerOptions);

                if (projectModelSnapshot is null)
                {
                    throw new SerializedDataException($"Serialized project data invalid at path: {projectPropertiesFilePath}");
                }

                projects.Add((Guid)projectModelSnapshot.GetId(), projectModelSnapshot);
            }
        }

        return projects;
    }

    public IEnumerable<Guid> GetAllProjectIds()
    {
        return GetAllProjectModelSnapshotsById().Keys;
    }

    public bool LoadIntoProjectProvider(Guid projectId)
    {
        var projectModelSnapshot = GetAllProjectModelSnapshotsById()
            .Where(e => e.Key == projectId)
            .Select(e => e.Value)
            .FirstOrDefault();

        if (projectModelSnapshot is null)
        {
            return false;
        }

        var project = ProjectBuilder.BuildModel(projectModelSnapshot);
        _projectProvider.CurrentProject = project;

        return true;
    }

    public async Task InitializeProjectDatabaseAsync(Guid projectId, CancellationToken cancellationToken)
    {
        using (var repo = new Repository(_repositoryPath))
        {
            var headCommitSha = repo.Commits.Select(c => c.Sha).FirstOrDefault();

            if (headCommitSha is not null)
            {
                var command = new InitializeDatabaseCommand(_repositoryPath, headCommitSha, projectId);
                var result = await _mediator.Send(command, cancellationToken);
            }
            else
            {
                throw new CommitNotFoundException($"No commits found for project Id '{projectId}' with which to initialize project database");
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
                    Password = _configuration.RemotePassword
                });

            foreach (Remote remote in repo.Network.Remotes)
            {
                IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
            }

            var signature = new LibGit2Sharp.Signature(
                new Identity(_configuration.RemoteUserName, _configuration.RemoteEmail), DateTimeOffset.Now);

            repo.Merge(repo.Branches.First().Tip, signature);
        }
        Console.WriteLine(logMessage);
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
                        Password = _configuration.RemotePassword
                    });

            var signature = new LibGit2Sharp.Signature(
                new Identity(_configuration.RemoteUserName, _configuration.RemoteEmail), DateTimeOffset.Now);

            Commands.Pull(repo, signature, options);
        }
    }

    public async Task MergeProjectLatestChangesAsync(CancellationToken cancellationToken)
    {
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
                // We are already at the latest
                _logger.LogInformation($"MergeLastestChangesAsync called, but repository has no commits yet");
                return;
            }

            headCommitSha = commitShas.First();
            lastMergedCommitShaIndex = commitShas.FindIndex(c => c == project.LastMergedCommitSha);
        }

        if (lastMergedCommitShaIndex == -1 && project.LastMergedCommitSha is not null)
        {
            // FIXME:  not sure what to do here.  Wrong commit tree or something?
            throw new CommitNotFoundException(project.LastMergedCommitSha);
        }

        if (lastMergedCommitShaIndex == 0)
        {
            // We are already at the latest
            _logger.LogInformation($"MergeLastestChangesAsync called, but already at latest commit '{project.LastMergedCommitSha}'");
            return;
        }

        var factory = new ProjectSnapshotFromGitFactory(_repositoryPath, _logger);

        var projectSnapshotLastMerged = (lastMergedCommitShaIndex == -1)
            ? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(project.Id)
            : factory.LoadSnapshot(project.LastMergedCommitSha!, project.Id);

        var projectSnapshotToMerge = factory.LoadSnapshot(headCommitSha, project.Id);

        // Create a backup snapshot of the current database:
        if (lastMergedCommitShaIndex > 0)
        {
            await CreateProjectBackupAsync(cancellationToken);
        }

        // Merge into the project database:
        var command = new MergeProjectSnapshotCommand(headCommitSha, projectSnapshotLastMerged, projectSnapshotToMerge, _logMergeOnly);
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();
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

    public async Task StageProjectChangesAsync(CancellationToken cancellationToken)
    {
        EnsureValidRepository(_repositoryPath);
        var project = EnsureCurrentProject();

        var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(project.Id);

        // Extract the latest from the project database:
        var command = new GetProjectSnapshotQuery();
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();

        // Save it to the local git directory:
        var factory = new ProjectSnapshotFilesFactory(Path.Combine(_repositoryPath, projectFolderName), _logger);
        factory.SaveSnapshot(result.Data!);

        // Stage the changes:  (or maybe have this as a separate step?)
        using (var repo = new Repository(_repositoryPath))
        {
            Commands.Stage(repo, projectFolderName);
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

        FileStatus[] fileStatuses = {
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

    public void CommitPushChanges(string commitMessage)
	{
        EnsureValidRepository(_repositoryPath);

        //GitHelper.RetrieveStatus(path, Logger, LibGit2Sharp.FileStatus.NewInIndex, LibGit2Sharp.FileStatus.ModifiedInIndex, LibGit2Sharp.FileStatus.RenamedInIndex, LibGit2Sharp.FileStatus.DeletedFromIndex);

        //_ = EnsureCurrentProject();

        // FIXME:  check to see if there are any changes to commit

        var userSignature = new Signature(
            _configuration.RemoteUserName,
            _configuration.RemoteEmail,
            DateTimeOffset.UtcNow
            );

        using (var repo = new Repository(_repositoryPath))
        {
            repo.Commit(commitMessage, userSignature, userSignature);

            Remote remote = repo.Network.Remotes[RemoteOrigin];
            var options = new PushOptions();
            options.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = _configuration.RemoteUserName, Password = _configuration.RemotePassword };
            repo.Network.Push(remote, @"refs/heads/master", options);
        }
    }
}

