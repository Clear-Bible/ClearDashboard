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
    public class GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler : IRequestHandler<
        GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery,
        RequestResult<IEnumerable<ParallelTokenizedCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<ParallelTokenizedCorpusId>>>
            Handle(GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery command, CancellationToken cancellationToken)
        {

            return Task.FromResult(
                new RequestResult<IEnumerable<ParallelTokenizedCorpusId>>
                (result: new List<ParallelTokenizedCorpusId>()
                {
                    new ParallelTokenizedCorpusId(new Guid())
                },
                success: true,
                message: "successful result from test"));
        }
    }
}
