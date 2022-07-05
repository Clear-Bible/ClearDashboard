using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery(string ProjectName, ParallelCorpusVersionId ParallelCorpusVersionId) 
        : ProjectRequestQuery<IEnumerable<ParallelTokenizedCorpusId>>(ProjectName);
}
