using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Alignment.DataServices.Features.Corpora;
using System;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
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
