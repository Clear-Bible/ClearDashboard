using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery,
        RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<EngineVerseMapping> engineVerseMappings,
            ParallelCorpusVersionId parallelCorpusVersionId,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedCorpusId sourceTokenizedCorpusId,
        TokenizedCorpusId targetTokenizedCorpusId,
        IEnumerable<EngineVerseMapping> engineVerseMappings,
        ParallelCorpusVersionId parallelCorpusVersionId,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId, TokenizedCorpusId targetTokenizedCorpusId, IEnumerable<EngineVerseMapping> engineVerseMappings, ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: use command.ParallelTokenizedCorpus to retrieve from ParallelTokenizedCorpus table and return
            //the TokenizedCorpusId for both and target and also
            //1. the result of gathering all the VerseMappings under parent parallelTokenizedCorpus.ParallelCorpusVersion to build an EngineVerseMapping list.
            //2. parent ParallelCorpusVersion's id
            //3. parent ParallelCorpusVersion's parent ParallelCorpusId

            return Task.FromResult(
                new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                    TokenizedCorpusId targetTokenizedCorpusId,
                    IEnumerable<EngineVerseMapping> engineVerseMappings,
                    ParallelCorpusVersionId parallelCorpusVersionId,
                    ParallelCorpusId parallelCorpusId)>
                (result: (new TokenizedCorpusId(new Guid()),
                        new TokenizedCorpusId(new Guid()),
                        new List<EngineVerseMapping>() {
                            new EngineVerseMapping(
                                new List<EngineVerseId>() {new EngineVerseId("MAT", 1, 1)},
                                new List<EngineVerseId>() {new EngineVerseId("MAT", 1, 1) })},
                        new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow),
                        new ParallelCorpusId(new Guid())),
                    success: true,
                    message: "successful result from test"));
        }

        

        
    }
}
