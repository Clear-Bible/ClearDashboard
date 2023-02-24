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

namespace ClearDashboard.Collaboration.Services;

public class CollaborationManager
{
	private readonly ILogger _logger;
	private readonly IMediator _mediator;
	private readonly IUserProvider _userProvider;
	private readonly IProjectProvider _projectProvider;
	private readonly string _repositoryPath = FilePathTemplates.ProjectBaseDirectory;

    // FIXME:  where should this come from?  User has to enter it somewhere?
    // Or its pre-setup somehow?  Maybe it can come from the IUserProvider?
    private readonly string _remoteUrl = "https://github.com/morleycb/snapshot_test.git";
    private readonly string _remoteUserName = "morleycb";
    private readonly string _remoteEmail = "chris.morley@clear.bible";
    private readonly string _remotePassword = "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";
    private readonly bool _logMergeOnly = true;

    public const string BackupsFolder = "Backups";

    public CollaborationManager(
		ILogger logger,
		IMediator mediator,
        IUserProvider userProvider,
		IProjectProvider projectProvider)
	{
		_logger = logger;
		_mediator = mediator;
		_userProvider = userProvider;
		_projectProvider = projectProvider;
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
                const string name = "origin";

                repo.Network.Remotes.Add(name, _remoteUrl);
                Remote remote = repo.Network.Remotes[name];

                var refSpec = repo.Config.Get<string>("remote", remote.Name, "fetch");
            }
        }
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
                        Username = _remoteUserName,
                        Password = _remotePassword
                    });

            var signature = new LibGit2Sharp.Signature(
                new Identity(_remoteUserName, _remoteEmail), DateTimeOffset.Now);

            Commands.Pull(repo, signature, options);
        }
    }

    public async Task MergeLatestChangesAsync(CancellationToken cancellationToken)
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
            ? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(project)
            : factory.LoadSnapshot(project.LastMergedCommitSha!, project.Id);

        var projectSnapshotToMerge = factory.LoadSnapshot(headCommitSha, project.Id);

        // Create a backup snapshot of the current database:
        await CreateBackupAsync(cancellationToken);

        // Merge into the project database:
        var command = new MergeProjectSnapshotCommand(projectSnapshotLastMerged, projectSnapshotToMerge, _logMergeOnly);
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();
    }

    public async Task CreateBackupAsync(CancellationToken cancellationToken)
    {
        var backupsPath = Path.Combine(_repositoryPath, BackupsFolder);
        Directory.CreateDirectory(backupsPath);

        var folderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(EnsureCurrentProject().Id) +
            DateTimeOffset.UtcNow.ToString("__yyyy-MM-dd_HH-mm-ss");

        // Extract the latest from the project database:
        var command = new GetProjectSnapshotQuery();
        var result = await _mediator.Send(command, cancellationToken);
        result.ThrowIfCanceledOrFailed();

        // Save it to the local backup directory:
        var factory = new ProjectSnapshotFilesFactory(Path.Combine(backupsPath, folderName), _logger);
        factory.SaveSnapshot(result.Data!);
    }

    public async Task StageChangesAsync(CancellationToken cancellationToken)
    {
        EnsureValidRepository(_repositoryPath);

        var projectFolderName = ProjectSnapshotFactoryCommon.ToProjectFolderName(EnsureCurrentProject().Id);

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

    public IEnumerable<string> RetrieveProjectStatus()
    {
        EnsureValidRepository(_repositoryPath);

        FileStatus[] fileStatuses = {
            LibGit2Sharp.FileStatus.NewInIndex,
            LibGit2Sharp.FileStatus.ModifiedInIndex,
            LibGit2Sharp.FileStatus.RenamedInIndex,
            LibGit2Sharp.FileStatus.DeletedFromIndex
        };

        var statuses = new List<string>();

        using (var repo = new Repository(_repositoryPath))
        {
            foreach (var item in repo.RetrieveStatus(new LibGit2Sharp.StatusOptions()))
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

        _ = EnsureCurrentProject();

        // FIXME:  check to see if there are any changes to commit

        var userSignature = new Signature(
            _remoteUserName,
            _remoteEmail,
            DateTimeOffset.UtcNow
            );

        using (var repo = new Repository(_repositoryPath))
        {
            repo.Commit(commitMessage, userSignature, userSignature);

            Remote remote = repo.Network.Remotes["origin"];
            var options = new PushOptions();
            options.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = _remoteUserName, Password = _remotePassword };
            repo.Network.Push(remote, @"refs/heads/master", options);
        }
    }
}

