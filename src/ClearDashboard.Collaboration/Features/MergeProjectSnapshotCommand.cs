using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Utils;

namespace ClearDashboard.Collaboration.Features;

public record MergeProjectSnapshotCommand(
    string CommitShaToMerge,
    ProjectSnapshot ProjectSnapshotLastMerged,
    ProjectSnapshot ProjectSnapshotToMerge,
    MergeMode MergeMode,
    bool UseLogOnlyMergeBehavior,
    IProgress<ProgressStatus> Progress) : ProjectRequestCommand<Unit>;
