using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class UpdateParallelCorpusCommandHandler : IRequestHandler<UpdateParallelCorpusCommand, RequestResult<Unit>>
    {
        public async Task<RequestResult<Unit>> Handle(UpdateParallelCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. find parallelCorpus based on request.ParallelCorpusId
            //2. Update the verse mappings

            var parallelCorpus = await ParallelCorpus.Get(new MediatorMock(), new ParallelCorpusId(new Guid()));


            return new RequestResult<Unit>
                    (result: Unit.Value,
                    success: true,
                    message: "successful result from test");
        }
    }
}
