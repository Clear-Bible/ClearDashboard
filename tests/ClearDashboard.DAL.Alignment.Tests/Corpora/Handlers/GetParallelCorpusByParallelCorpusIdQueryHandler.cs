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
        RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>>
    {
        public Task<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>>
            Handle(GetParallelCorpusByParallelCorpusIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an EngineVerseMapping list.
            //2. associated source and target TokenizedCorpusId

            return Task.FromResult(
                new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                    TokenizedCorpusId targetTokenizedCorpusId,
                    IEnumerable<VerseMapping> verseMappings,
                    ParallelCorpusId parallelCorpusId)>
                (result: (new TokenizedCorpusId(new Guid()),
                    new TokenizedCorpusId(new Guid()), 
                    new List<VerseMapping>() { 
                        new VerseMapping(
                            new List<Verse>() {new Verse("MAT", 1, 1)}, 
                            new List<Verse>() {new Verse("MAT", 1, 1) })},
                    new ParallelCorpusId(new Guid())),
                success: true,
                message: "successful result from test"));
        }
    }
}
