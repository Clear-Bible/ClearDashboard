using System;
using System.Linq;
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
        public async Task<RequestResult<ParallelCorpusId>>
            Handle(CreateParallelCorpusCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. Create a new record in ParallelCorpus, save ParallelCorpusId
            //2. Create VerseMappings with verses
            //3. return created ParallelCorpus based on ParallelCorpusId
            await Task.CompletedTask;

            return new RequestResult<ParallelCorpusId>
                    (result: new ParallelCorpusId(new Guid()),
                    success: true,
                    message: "successful result from test");
        }
    }
}
