using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record UpdateParallelCorpusCommand(
        IEnumerable<VerseMapping> VerseMappings,
        ParallelCorpusId ParallelCorpusId) : ProjectRequestCommand<Unit>;
}
