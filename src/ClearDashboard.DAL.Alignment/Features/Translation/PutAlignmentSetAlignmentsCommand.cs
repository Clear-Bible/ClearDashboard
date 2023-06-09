using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record PutAlignmentSetAlignmentsCommand(
        AlignmentSetId AlignmentSetId,
        IEnumerable<Alignment.Translation.Alignment> Alignments) : ProjectRequestCommand<Unit>;
} 