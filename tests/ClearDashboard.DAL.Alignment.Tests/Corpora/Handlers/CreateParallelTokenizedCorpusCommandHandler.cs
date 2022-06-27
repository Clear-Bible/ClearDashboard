using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;
using System;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
{
    public class CreateParallelTokenizedCorpusCommandHandler : IRequestHandler<
        CreateParallelTokenizedCorpusCommand,
        RequestResult<ParallelTokenizedCorpusId>>
    {
        public Task<RequestResult<ParallelTokenizedCorpusId>>
            Handle(CreateParallelTokenizedCorpusCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //Create a new record in ParallelTokenizedCorpus table with command.sourceCorpusIdVersionId and command.targetCorpusIdVersionId and parent command.ParallelCorpusVersionId

            return Task.FromResult(
                new RequestResult<ParallelTokenizedCorpusId>
                (result: new ParallelTokenizedCorpusId(new Guid()),
                success: true,
                message: "successful result from test"));
        }
    }
}
