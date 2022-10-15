using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Denormalization
{
    public record DenormalizeAlignmentTopTargetsCommand(
        Guid AlignmentSetId) : ProjectRequestCommand<object>;
}
