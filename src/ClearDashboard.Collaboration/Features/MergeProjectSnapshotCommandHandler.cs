using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Merge;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

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
            ? new MergeBehaviorLogOnly(Logger!, new MergeCache(), request.Progress)
            : new MergeBehaviorApply(Logger, ProjectDbContext!, new MergeCache(), request.Progress))
        {
            await mergeBehavior.MergeStartAsync(cancellationToken);
            try
            {
                request.Progress.Report(new ProgressStatus(0, "Calculating differences between last merged and latest commit"));

                var projectDifferences = new ProjectDifferences(request.ProjectSnapshotLastMerged, request.ProjectSnapshotToMerge, cancellationToken);

                if (!request.UseLogOnlyMergeBehavior)
                {
                    projectInDatabase.LastMergedCommitSha = request.CommitShaToMerge;
                    if (!projectDifferences.HasDifferences)
                    {
                        await ProjectDbContext.SaveChangesAsync(cancellationToken);
                        request.Progress.Report(new ProgressStatus(0, "No differences found.  Merge complete!"));

                        return new RequestResult<Unit>(Unit.Value);
                    }
                }

                BuilderContext builderContext = new(ProjectDbContext);
                var currentProjectSnapshot = GetProjectSnapshotQueryHandler.LoadSnapshot(builderContext, cancellationToken);

                request.Progress.Report(new ProgressStatus(0, "Starting merge..."));

                var mergeContext = new MergeContext(ProjectDbContext!.UserProvider, Logger, mergeBehavior, request.MergeMode);
                var merger = new Merger(mergeContext);
                await merger.MergeAsync(projectDifferences, currentProjectSnapshot, request.ProjectSnapshotToMerge, request.ProjectSnapshotLastMerged, cancellationToken);

                await mergeBehavior.MergeEndAsync(cancellationToken);
                // FIXME:  temporary, for testing:
                //await MergeContext.MergeBehavior.MergeErrorAsync(cancellationToken);

                request.Progress.Report(new ProgressStatus(0, "MergeDialog_MergeCompleteReloadingInterface"));

                if (mergeContext.FireAlignmentDenormalizationEvent)
                {
                    await _mediator.Publish(new AlignmentAddedRemovedEvent(Enumerable.Empty<Models.Alignment>(), null), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                await mergeBehavior.MergeErrorAsync(CancellationToken.None);
                return new RequestResult<Unit>
                {
                    Message = "Operation Canceled",
                    Success = false,
                    Canceled = true
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Exception thrown in handler '{GetType().Name}'");

                await mergeBehavior.MergeErrorAsync(CancellationToken.None);
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
