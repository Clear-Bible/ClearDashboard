using ClearDashboard.DAL.Alignment.BackgroundServices;
using ClearDashboard.DAL.CQRS.Features;
using SIL.Machine.Utils;

namespace ClearDashboard.DAL.Alignment.Features.Denormalization
{
    public record DenormalizeAlignmentTopTargetsCommand(
        Guid AlignmentSetId, ILongRunningProgress<ProgressStatus> Progress) : ProjectRequestCommand<int>;
}
