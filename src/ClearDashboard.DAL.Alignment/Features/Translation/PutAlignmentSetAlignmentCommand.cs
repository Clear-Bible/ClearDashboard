using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record PutAlignmentSetAlignmentCommand(
        AlignmentSetId AlignmentSetId,
        Alignment.Translation.Alignment Alignment) : ProjectRequestCommand<object>;
}