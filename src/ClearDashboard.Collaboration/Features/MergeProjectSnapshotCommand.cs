using ClearDashboard.Collaboration;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
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
    bool UseLogOnlyMergeBehavior) : ProjectRequestCommand<Unit>;
