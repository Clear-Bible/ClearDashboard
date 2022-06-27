using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearDashboard.DAL.CQRS;


namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
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
