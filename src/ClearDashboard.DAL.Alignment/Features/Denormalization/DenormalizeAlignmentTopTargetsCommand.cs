using ClearDashboard.DAL.Alignment.BackgroundServices;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using SIL.Machine.Utils;

namespace ClearDashboard.DAL.Alignment.Features.Denormalization
{
    /// <summary>
    /// FIXME:  perhaps AlignmentTypesToInclude used in denormalization
    /// should be a dashboard setting/configuration
    /// </summary>
    /// <param name="AlignmentSetId"></param>
    /// <param name="Progress"></param>
    /// <param name="AlignmentTypesToInclude"></param>
    public record DenormalizeAlignmentTopTargetsCommand(
        Guid AlignmentSetId, ILongRunningProgress<ProgressStatus> Progress, AlignmentTypes AlignmentTypesToInclude = AlignmentTypes.AssignedAndUnverifiedNotOtherwiseIncluded) : ProjectRequestCommand<int>;
}
