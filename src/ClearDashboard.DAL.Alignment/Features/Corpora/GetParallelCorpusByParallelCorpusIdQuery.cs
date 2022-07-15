using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetParallelCorpusByParallelCorpusIdQuery(ParallelCorpusId  ParallelCorpusId)
        : ProjectRequestQuery<
            (TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId, 
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>;
}
