using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    /// <summary>
    /// Deletes a ParallelCorpus, as well as any/all downstream dependent 
    /// instances (e.g. TranslationSets, Translations etc)
    /// </summary>
    /// <param name="ParallelCorpusId"></param>
    public record DeleteParallelCorpusByParallelCorpusIdCommand(ParallelCorpusId ParallelCorpusId) : ProjectRequestCommand<Unit>;
}
