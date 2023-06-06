using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Features;

public record MergeProjectSnapshotCommand(
    string CommitShaToMerge,
    ProjectSnapshot ProjectSnapshotLastMerged,
    ProjectSnapshot ProjectSnapshotToMerge,
    MergeMode MergeMode,
    bool UseLogOnlyMergeBehavior,
    IProgress<ProgressStatus> Progress) : ProjectRequestCommand<Unit>;
