using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Merge;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Features;
public class InitializeDatabaseCommandHandler : IRequestHandler<InitializeDatabaseCommand, RequestResult<Unit>>
{
    private ProjectDbContextFactory _projectNameDbContextFactory { get; init; }
    private IUserProvider _userProvider { get; init; }
    private ILogger _logger { get; init; }
    public InitializeDatabaseCommandHandler(
        ProjectDbContextFactory projectNameDbContextFactory,
        IUserProvider userProvider,
        ILogger<InitializeDatabaseCommandHandler> logger)
    {
        _projectNameDbContextFactory = projectNameDbContextFactory;
        _userProvider = userProvider;
        _logger = logger;
    }

    public async Task<RequestResult<Unit>> Handle(InitializeDatabaseCommand request, CancellationToken cancellationToken)
    {
        Action<ILogger>? errorCleanupAction = null;
        try
        {
            var factory = new ProjectSnapshotFromGitFactory(request.RepositoryPath, _logger);

            var projectModelSnapshot = factory.LoadProject(request.CommitSha, request.ProjectId);
            var userModelSnapshots = factory.LoadUsers(request.CommitSha, request.ProjectId, cancellationToken);

            request.Progress.Report(new ProgressStatus(0, "MergeDialog_CreatingDatabase"));

            await using (var requestScope = _projectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var projectContext = await _projectNameDbContextFactory!.GetDatabaseContext(
                    (string)projectModelSnapshot[nameof(Models.Project.ProjectName)]!,
                    true,
                    requestScope).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    errorCleanupAction = GetErrorCleanupAction(projectContext, _logger);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (projectContext.Projects.Any())
                {
                    // Already initialized
                    return new RequestResult<Unit>(Unit.Value);
                }

                request.Progress.Report(new ProgressStatus(0, "Adding snapshot users and project to database"));

                await using MergeBehaviorBase mergeBehavior = new MergeBehaviorApply(_logger, projectContext, new MergeCache(), request.Progress);
                await mergeBehavior.MergeStartAsync(cancellationToken);
                try
                {
                    // Insert user(s) first because Project will have a foreign
                    // key reference to one of them:
                    mergeBehavior.StartInsertModelCommand(userModelSnapshots.First());
                    foreach (var u in userModelSnapshots)
                    {
                        await mergeBehavior.RunInsertModelCommand(u, cancellationToken);
                    }
                    mergeBehavior.CompleteInsertModelCommand(userModelSnapshots.First().EntityType);

                    mergeBehavior.StartInsertModelCommand(projectModelSnapshot);
                    await mergeBehavior.RunInsertModelCommand(projectModelSnapshot, cancellationToken);
                    mergeBehavior.CompleteInsertModelCommand(projectModelSnapshot.EntityType);

                    if (_userProvider.CurrentUser is not null && !userModelSnapshots
                        .Select(e => (Guid)e.GetId())
                        .Contains(_userProvider.CurrentUser.Id))
                    {
                        request.Progress.Report(new ProgressStatus(0, "Adding current user to database"));
                        projectContext.Users.Add(_userProvider.CurrentUser);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (request.IncludeMerge)
                    {
                        request.Progress.Report(new ProgressStatus(0, "Creating/loading project snapshots for merge"));

                        var project = projectContext.Projects.Where(e => e.Id == (Guid)projectModelSnapshot.GetId()).First();

                        var projectSnapshotLastMerged = GetProjectSnapshotQueryHandler.LoadSnapshot(new Builder.BuilderContext(projectContext), cancellationToken);
                        var projectSnapshotToMerge = factory.LoadSnapshot(request.CommitSha, project.Id, cancellationToken);

                        request.Progress.Report(new ProgressStatus(0, "Calculating differences in latest commit"));
                        var projectDifferences = new ProjectDifferences(projectSnapshotLastMerged, projectSnapshotToMerge, cancellationToken);

                        request.Progress.Report(new ProgressStatus(0, "Starting merge..."));

                        var merger = new Merger(new MergeContext(projectContext.UserProvider, _logger, mergeBehavior, MergeMode.RemoteOverridesLocal));
                        await merger.MergeAsync(projectDifferences, projectSnapshotLastMerged, projectSnapshotToMerge, cancellationToken);

                        request.Progress.Report(new ProgressStatus(0, "Merge complete! - Continuing Initialization"));

                        project.LastMergedCommitSha = request.CommitSha;
                    }

                    await mergeBehavior.MergeEndAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown in handler '{GetType().Name}'");

                    await mergeBehavior.MergeErrorAsync(CancellationToken.None);
                    errorCleanupAction = GetErrorCleanupAction(projectContext, _logger);
                    throw;
                }
            }

            return new RequestResult<Unit>(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            return new RequestResult<Unit>
            {
                Message = "Operation Canceled",
                Success = false,
                Canceled = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception thrown in handler '{GetType().Name}'");

            var innerExceptionMessage = (ex.InnerException is not null) ?
                $" (inner exception message: {ex.InnerException.Message})" :
                "";
            request.Progress.Report(new SIL.Machine.Utils.ProgressStatus(0, $"Unable to initialize database for project '{request.ProjectId}', commit '{request.CommitSha}':  exception type: {ex.GetType().Name}, having message: {ex.Message}{innerExceptionMessage}"));
            return new RequestResult<Unit>
            {
                Message = $"Unable to initialize database for project '{request.ProjectId}', commit '{request.CommitSha}':  exception type: {ex.GetType().Name}, having message: {ex.Message}{innerExceptionMessage}",
                Success = false
            };
        }
        finally
        {
            if (errorCleanupAction is not null)
            {
                errorCleanupAction(_logger);
            }
        }
    }

    public static Action<ILogger>? GetErrorCleanupAction(ProjectDbContext? projectContext, ILogger logger)
    {
        if (projectContext is not null)
        {
            var dbConnection = projectContext.Database.GetDbConnection();
            if (dbConnection is SqliteConnection && 
                projectContext.OptionsBuilder is SqliteProjectDbContextOptionsBuilder<ProjectDbContext> optionsBuilder)
            {
                var deleteDatabasePath = optionsBuilder.DatabaseDirectory;
                return (logger) =>
                {
                    try
                    {
                        // Deleting the database directory without first doing this will
                        // fail with "file in use".  Notice that is isn't connection specific -
                        // this should never be run except when no project is currently 
                        // open.
                        SqliteConnection.ClearAllPools();
                        Directory.Delete(deleteDatabasePath, true);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error attempting to clean up SqliteDatabase at path '{deleteDatabasePath}':  {ex.Message}");
                    }
                };
            }
            else
            {
                // Could throw a NotImplementedException, but could hide the original
                // exception that was the trigger for calling this method
                logger.LogError($"Unable to clean up database after InitializeDatabaseCommandHandler error - not a Sqlite database?");
            }
        }

        return null;
    }
}
