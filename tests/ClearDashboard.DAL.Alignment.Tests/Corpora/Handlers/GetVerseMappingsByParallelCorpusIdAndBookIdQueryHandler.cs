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
    public class GetVerseMappingsByParallelCorpusIdAndBookIdQueryHandler : IRequestHandler<
        GetVerseMappingsByParallelCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<VerseMapping>>>
    {
        public Task<RequestResult<IEnumerable<VerseMapping>>>
            Handle(GetVerseMappingsByParallelCorpusIdAndBookIdQuery command, CancellationToken cancellationToken)
        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an EngineVerseMapping list.
            //2. associated source and target TokenizedCorpusId

            return Task.FromResult(
                new RequestResult<IEnumerable<VerseMapping>>
                (result: 
                    new List<VerseMapping>() {
                        new VerseMapping(
                            new List<Verse>() {new Verse("MAT", 1, 1)},
                            new List<Verse>() {new Verse("MAT", 1, 1)}
                        )
                    },
                success: true,
                message: "successful result from test"));
        }
    }
}
