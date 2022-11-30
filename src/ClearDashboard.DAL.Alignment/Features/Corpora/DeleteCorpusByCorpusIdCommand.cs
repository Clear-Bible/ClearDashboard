using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    /// <summary>
    /// Deletes a corpus, as well as any/all downstream dependent 
    /// instances (e.g. TokenizedCorpora, ParallelCorpora etc)
    /// </summary>
    /// <param name="CorpusId"></param>
    public record DeleteCorpusByCorpusIdCommand(CorpusId CorpusId) : ProjectRequestCommand<Unit>;
}
