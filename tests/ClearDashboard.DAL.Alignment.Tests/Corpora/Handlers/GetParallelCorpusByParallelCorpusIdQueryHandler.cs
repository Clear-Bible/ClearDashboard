using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetParallelCorpusByParallelCorpusIdQueryHandler : IRequestHandler<
        GetParallelCorpusByParallelCorpusIdQuery,
        RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
            TokenizedTextCorpusId targetTokenizedCorpusId,
            ParallelCorpusId parallelCorpusId)>>
    {
        public Task<RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
            TokenizedTextCorpusId targetTokenizedCorpusId,
            ParallelCorpusId parallelCorpusId)>>
            Handle(GetParallelCorpusByParallelCorpusIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an EngineVerseMapping list.
            //2. associated source and target TokenizedCorpusId

            return Task.FromResult(
                new RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
                    TokenizedTextCorpusId targetTokenizedCorpusId,
                    ParallelCorpusId parallelCorpusId)>
                (result: (new TokenizedTextCorpusId(new Guid()),
                    new TokenizedTextCorpusId(new Guid()), 
                    new ParallelCorpusId(new Guid())),
                success: true,
                message: "successful result from test"));
        }
    }
}
