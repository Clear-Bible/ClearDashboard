using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetAllParallelCorpusVersionIdsQueryHandler : IRequestHandler<
        GetAllParallelCorpusVersionIdsQuery,
        RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>>
    {
        public Task<RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>>
            Handle(GetAllParallelCorpusVersionIdsQuery command, CancellationToken cancellationToken)
        {

            return Task.FromResult(
                new RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>
                (result: new List<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>()
                {
                    (new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow), new ParallelCorpusId(new Guid()))
                },
                success: true,
                message: "successful result from test"));
        }
    }
}
