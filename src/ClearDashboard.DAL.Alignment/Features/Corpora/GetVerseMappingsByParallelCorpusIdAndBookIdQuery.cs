using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetVerseMappingsByParallelCorpusIdAndBookIdQuery(
        ParallelCorpusId  ParallelCorpusId, 
        string? BookId) : ProjectRequestQuery<IEnumerable<VerseMapping>>;
}
