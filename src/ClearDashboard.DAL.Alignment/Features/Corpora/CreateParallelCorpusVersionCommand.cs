using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelCorpusVersionCommand(ParallelCorpusId ParallelCorpusId, EngineParallelTextCorpus EngineParallelTextCorpus) : ProjectRequestCommand<ParallelCorpusVersionId>;
}
