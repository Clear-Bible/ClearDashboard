using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class CreateParallelCorpusCommandHandler : IRequestHandler<
        CreateParallelCorpusCommand,
        RequestResult<ParallelCorpusId>>
    {
        public Task<RequestResult<ParallelCorpusId>>
            Handle(CreateParallelCorpusCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //Create a new record in ParallelCorpus and return its id.

            return Task.FromResult(
                new RequestResult<ParallelCorpusId>
                (result: new ParallelCorpusId(new Guid()),
                success: true,
                message: "successful result from test"));
        }
    }
}
