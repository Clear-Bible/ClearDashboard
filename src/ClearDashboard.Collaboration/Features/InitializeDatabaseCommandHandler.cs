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

namespace ClearDashboard.Collaboration.Features;
public class InitializeDatabaseCommandHandler : ProjectDbContextCommandHandler<
    InitializeDatabaseCommand,
    RequestResult<Unit>,
    Unit>
{
    public InitializeDatabaseCommandHandler(ProjectDbContextFactory projectNameDbContextFactory,
        IProjectProvider projectProvider,
        ILogger<InitializeDatabaseCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
    {
    }

    protected override async Task<RequestResult<Unit>> SaveDataAsync(
    InitializeDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ProjectSnapshotFromGitFactory(request.repositoryPath, Logger);

            var projectModelSnapshot = factory.LoadProject(request.commitSha, request.projectId);
            var userModelSnapshots = factory.LoadUsers(request.commitSha, request.projectId);

            await using (var requestScope = ProjectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var projectContext = await ProjectNameDbContextFactory!.GetDatabaseContext(
                    (string)projectModelSnapshot[nameof(Models.Project.ProjectName)]!,
                    true,
                    requestScope).ConfigureAwait(false);

                if (projectContext.Projects.Any())
                {
                    // Already initialized
                    return new RequestResult<Unit>(Unit.Value);
                }

                await using (MergeBehaviorBase mergeBehavior = new MergeBehaviorApply(Logger, projectContext, new MergeCache()))
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

                            var merger = new Merger(new MergeContext(projectContext.UserProvider, Logger, mergeBehavior, true));
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
        catch (Exception ex)
        {
            return new RequestResult<Unit>
            (
                success: false,
                message: $"Unable to initialize database for project '{request.projectId}', commit '{request.commitSha}':  {ex.Message}"
            );
        }
    }
}
