using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record CreateAlignmentSetCommand(
        IEnumerable<Alignment.Translation.Alignment> Alignments,
        string? DisplayName,
        string SmtModel,
        bool IsSyntaxTreeAlignerRefined,
        Dictionary<string, object> Metadata,
        ParallelCorpusId ParallelCorpusId) : ProjectRequestCommand<AlignmentSet>;
}
