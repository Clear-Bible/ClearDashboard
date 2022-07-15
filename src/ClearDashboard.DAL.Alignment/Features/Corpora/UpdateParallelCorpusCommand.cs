using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record UpdateParallelCorpusCommand(
        IEnumerable<VerseMapping> VerseMappings,
        ParallelCorpusId ParallelCorpusId) : ProjectRequestCommand<ParallelCorpus>;
}
