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
    public class GetAllParallelCorpusIdsQueryHandler : IRequestHandler<
        GetAllParallelCorpusIdsQuery,
        RequestResult<IEnumerable<ParallelCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<ParallelCorpusId>>>
            Handle(GetAllParallelCorpusIdsQuery command, CancellationToken cancellationToken)
        {

            return Task.FromResult(
                new RequestResult<IEnumerable<ParallelCorpusId>>
                (result: new List<ParallelCorpusId>()
                {
                    new ParallelCorpusId(new Guid())
                },
                success: true,
                message: "successful result from test"));
        }
    }
}
