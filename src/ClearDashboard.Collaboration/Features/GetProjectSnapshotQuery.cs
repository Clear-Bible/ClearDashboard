using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.Collaboration.Features;

public record GetProjectSnapshotQuery() : ProjectRequestQuery<ProjectSnapshot>;
