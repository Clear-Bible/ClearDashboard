using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.Collaboration.Merge;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.AspNet.SignalR.Client.Http;
using ClearDashboard.DAL.CQRS.Features.Features;

namespace ClearDashboard.Collaboration.Features;
public class InitializeDatabaseCommandHandler : IRequestHandler<InitializeDatabaseCommand, RequestResult<Unit>>
{
    private ProjectDbContextFactory _projectNameDbContextFactory { get; init; }
    private ILogger _logger { get; init; }
    public InitializeDatabaseCommandHandler(ProjectDbContextFactory projectNameDbContextFactory,
        ILogger<InitializeDatabaseCommandHandler> logger)
    {
        _projectNameDbContextFactory = projectNameDbContextFactory;
        _logger = logger;
    }

    public async Task<RequestResult<Unit>> Handle(InitializeDatabaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ProjectSnapshotFromGitFactory(request.repositoryPath, _logger);

            var projectModelSnapshot = factory.LoadProject(request.commitSha, request.projectId);
            var userModelSnapshots = factory.LoadUsers(request.commitSha, request.projectId);

            await using (var requestScope = _projectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var projectContext = await _projectNameDbContextFactory!.GetDatabaseContext(
                    (string)projectModelSnapshot[nameof(Models.Project.ProjectName)]!,
                    true,
                    requestScope).ConfigureAwait(false);

                if (projectContext.Projects.Any())
                {
                    // Already initialized
                    return new RequestResult<Unit>(Unit.Value);
                }

                await using (MergeBehaviorBase mergeBehavior = new MergeBehaviorApply(_logger, projectContext, new MergeCache()))
                {
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

                        if (request.includeMerge)
                        {
                            var project = projectContext.Projects.Where(e => e.Id == (Guid)projectModelSnapshot.GetId()).First();

                            var projectSnapshotLastMerged = GetProjectSnapshotQueryHandler.LoadSnapshot(new Builder.BuilderContext(projectContext));
                            var projectSnapshotToMerge = factory.LoadSnapshot(request.commitSha, project.Id);
                            var projectDifferences = new ProjectDifferences(projectSnapshotLastMerged, projectSnapshotToMerge);

                            var merger = new Merger(new MergeContext(projectContext.UserProvider, _logger, mergeBehavior, true));
                            await merger.MergeAsync(projectDifferences, projectSnapshotLastMerged, projectSnapshotToMerge, cancellationToken);

                            project.LastMergedCommitSha = request.commitSha;
                        }

                        await mergeBehavior.MergeEndAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await mergeBehavior.MergeErrorAsync(cancellationToken);

                        return new RequestResult<Unit>
                        (
                            success: false,
                            message: $"Unable to add project + user data to newly initialized database:  {ex.Message}"
                        );
                    }
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
            var innerExceptionMessage = (ex.InnerException is not null) ?
                $" (inner exception message: {ex.InnerException.Message})" :
                "";
            return new RequestResult<Unit>
            {
                Message = $"Unable to initialize database for project '{request.projectId}', commit '{request.commitSha}':  exception type: {ex.GetType().Name}, having message: {ex.Message}{innerExceptionMessage}",
                Success = false
            };
        }
    }
}
