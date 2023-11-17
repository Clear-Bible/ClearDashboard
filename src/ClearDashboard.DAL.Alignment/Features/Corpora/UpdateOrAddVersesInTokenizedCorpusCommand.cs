using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record UpdateOrAddVersesInTokenizedCorpusCommand(
        TokenizedTextCorpusId TokenizedTextCorpusId,
        ITextCorpus TextCorpus,
        IEnumerable<string> ExistingBookIds,
        List<AlignmentSetId> AlignmentSetsToRedo) : ProjectRequestCommand<IEnumerable<string>>;
}
