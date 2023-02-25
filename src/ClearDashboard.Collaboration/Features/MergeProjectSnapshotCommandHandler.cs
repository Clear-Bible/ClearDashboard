using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Merge;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Features;
public class MergeProjectSnapshotCommandHandler : ProjectDbContextCommandHandler<
    MergeProjectSnapshotCommand,
    RequestResult<Unit>,
    Unit>
{
    public MergeProjectSnapshotCommandHandler(ProjectDbContextFactory projectNameDbContextFactory,
        IProjectProvider projectProvider,
        ILogger<MergeProjectSnapshotCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
    {
    }

    protected override async Task<RequestResult<Unit>> SaveDataAsync(
        MergeProjectSnapshotCommand request,
        CancellationToken cancellationToken)
    {

        var projectDifferences = new ProjectDifferences(request.ProjectSnapshotLastMerged, request.ProjectSnapshotToMerge);

        if (request.UseLogOnlyMergeBehavior)
        {
            var backupPath = Path.Combine(FilePathTemplates.ProjectBaseDirectory, CollaborationManager.BackupsFolder);
            projectDifferences.Serialize(backupPath);
        }

        MergeBehaviorBase mergeBehavior = request.UseLogOnlyMergeBehavior
            ? new MergeBehaviorLogOnly(Logger!)
            : new MergeBehaviorApply(Logger, ProjectDbContext!);

        await using (var mergeContext = new MergeContext(ProjectDbContext!.UserProvider, Logger, mergeBehavior))
        {
            var merger = new Merger(mergeContext);
            await merger.MergeAsync(projectDifferences, request.ProjectSnapshotLastMerged, request.ProjectSnapshotToMerge, cancellationToken);
        }

        // Return some sort of merge results?  The differences?  
        return new RequestResult<Unit>(Unit.Value);
    }
}
