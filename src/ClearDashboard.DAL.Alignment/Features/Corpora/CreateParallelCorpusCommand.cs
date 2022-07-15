using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelCorpusCommand(
        TokenizedCorpusId SourceTokenizedCorpusId,
        TokenizedCorpusId TargetTokenizedCorpusId,
        IEnumerable<VerseMapping> VerseMappings) : ProjectRequestCommand<ParallelCorpus>;
}
