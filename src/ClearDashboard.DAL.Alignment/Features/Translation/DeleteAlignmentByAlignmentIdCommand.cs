using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record DeleteAlignmentByAlignmentIdCommand(
        AlignmentId AlignmentId) : ProjectRequestCommand<Unit>;
}