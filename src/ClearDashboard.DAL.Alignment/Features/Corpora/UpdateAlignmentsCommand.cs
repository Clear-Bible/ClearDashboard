using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record UpdateAlignmentsCommand(
        AlignmentSetId AlignmentSetsToRedo,
        TrainSmtModelSet TrainSmtModelSet,
        IEnumerable<EngineParallelTextRow> OldParallelTextRows
        ) : ProjectRequestCommand<AlignmentSet>;
}
