using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllParallelCorpusVersionIdsQuery(string ProjectName) 
        : ProjectRequestQuery<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>(ProjectName);
}
