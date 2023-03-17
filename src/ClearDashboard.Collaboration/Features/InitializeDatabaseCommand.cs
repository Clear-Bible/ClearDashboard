using ClearDashboard.Collaboration;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Features;

public record InitializeDatabaseCommand(
    string RepositoryPath,
    string CommitSha,
    Guid ProjectId,
    bool IncludeMerge,
    IProgress<ProgressStatus> Progress) : ProjectRequestCommand<Unit>;
