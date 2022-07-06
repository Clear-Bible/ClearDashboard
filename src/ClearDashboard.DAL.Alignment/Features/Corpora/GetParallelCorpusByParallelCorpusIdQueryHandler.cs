using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetParallelCorpusByParallelCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetParallelCorpusByParallelCorpusIdQuery,
        RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<EngineVerseMapping> engineVerseMappings,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedCorpusId sourceTokenizedCorpusId,
        TokenizedCorpusId targetTokenizedCorpusId,
        IEnumerable<EngineVerseMapping> engineVerseMappings,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelCorpusByParallelCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelCorpusByParallelCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId, 
            IEnumerable<EngineVerseMapping> engineVerseMappings, 
            ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelCorpusByParallelCorpusIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an EngineVerseMapping list.
            //2. associated source and target TokenizedCorpusId

            return Task.FromResult(
                new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                    TokenizedCorpusId targetTokenizedCorpusId,
                    IEnumerable<EngineVerseMapping> engineVerseMappings,
                    ParallelCorpusId parallelCorpusId)>
                (result: (new TokenizedCorpusId(new Guid()),
                        new TokenizedCorpusId(new Guid()),
                        new List<EngineVerseMapping>() {
                            new EngineVerseMapping(
                                new List<EngineVerseId>() {new EngineVerseId("MAT", 1, 1)},
                                new List<EngineVerseId>() {new EngineVerseId("MAT", 1, 1) })},
                        new ParallelCorpusId(new Guid())),
                    success: true,
                    message: "successful result from test"));
        }

        

        
    }
}
