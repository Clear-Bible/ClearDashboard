using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Utils;

namespace ClearDashboard.Collaboration.Features;

public record InitializeDatabaseCommand(
    string RepositoryPath,
    string CommitSha,
    Guid ProjectId,
    bool IncludeMerge,
    IProgress<ProgressStatus> Progress) : ProjectRequestCommand<Unit>;
