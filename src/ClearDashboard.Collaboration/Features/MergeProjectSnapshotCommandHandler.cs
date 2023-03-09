using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.DifferenceModel;
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
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Features.Events;

namespace ClearDashboard.Collaboration.Features;
public class MergeProjectSnapshotCommandHandler : ProjectDbContextCommandHandler<
    MergeProjectSnapshotCommand,
    RequestResult<Unit>,
    Unit>
{
    private readonly IMediator _mediator;

    public MergeProjectSnapshotCommandHandler(IMediator mediator,
        ProjectDbContextFactory projectNameDbContextFactory,
        IProjectProvider projectProvider,
        ILogger<MergeProjectSnapshotCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<Unit>> SaveDataAsync(
        MergeProjectSnapshotCommand request,
        CancellationToken cancellationToken)
    {
        var projectId = (Guid)request.ProjectSnapshotToMerge.Project.GetId();
        var projectInDatabase = ProjectDbContext.Projects.Where(e => e.Id == projectId).FirstOrDefault();

        if (projectInDatabase is null)
        {
            return new RequestResult<Unit>
            (
                success: false,
                message: $"Unable to apply differences - project '{projectId}' not found in local database"
            );
        }

        await using (MergeBehaviorBase mergeBehavior = request.UseLogOnlyMergeBehavior
            ? new MergeBehaviorLogOnly(Logger!, new MergeCache())
            : new MergeBehaviorApply(Logger, ProjectDbContext!, new MergeCache()))
        {
            await mergeBehavior.MergeStartAsync(cancellationToken);
            try
            {
                var projectDifferences = new ProjectDifferences(request.ProjectSnapshotLastMerged, request.ProjectSnapshotToMerge);

                if (!request.UseLogOnlyMergeBehavior)
                {
                    projectInDatabase.LastMergedCommitSha = request.CommitShaToMerge;
                }

                BuilderContext builderContext = new(ProjectDbContext);
                var currentProjectSnapshot = GetProjectSnapshotQueryHandler.LoadSnapshot(builderContext);

                var mergeContext = new MergeContext(ProjectDbContext!.UserProvider, Logger, mergeBehavior, request.RemoteOverridesLocal);
                var merger = new Merger(mergeContext);
                await merger.MergeAsync(projectDifferences, currentProjectSnapshot, request.ProjectSnapshotToMerge, cancellationToken);

                await mergeBehavior.MergeEndAsync(cancellationToken);
                // FIXME:  temporary, for testing:
                //await MergeContext.MergeBehavior.MergeErrorAsync(cancellationToken);

                if (mergeContext.FireAlignmentDenormalizationEvent)
                {
                    await _mediator.Publish(new AlignmentAddedRemovedEvent(Enumerable.Empty<Models.Alignment>(), null), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await mergeBehavior.MergeErrorAsync(cancellationToken);
                Logger.LogInformation($"Exception of type '{ex.GetType().ShortDisplayName()}': {ex.Message}");

                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Unable to apply differences due to exception of type '{ex.GetType().Name}':  {ex.Message}"
                );
            }
        }

        // Return some sort of merge results?  The differences?  
        return new RequestResult<Unit>(Unit.Value);
    }
}
