using System.Data.Entity;
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
            IEnumerable<Models.VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedCorpusId sourceTokenizedCorpusId,
        TokenizedCorpusId targetTokenizedCorpusId,
        IEnumerable<Models.VerseMapping> verseMappings,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelCorpusByParallelCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelCorpusByParallelCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId, 
            IEnumerable<Models.VerseMapping> verseMappings, 
            ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelCorpusByParallelCorpusIdQuery request, CancellationToken cancellationToken)

        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an VerseMapping list.
            //2. associated source and target TokenizedCorpusId

            var parallelCorpus =
                await ProjectDbContext.ParallelCorpa
                    .Include(pc => pc.VerseMappings)
                    .FirstOrDefaultAsync(pc => pc.Id == request.ParallelCorpusId.Id,
                        cancellationToken);

            //var verseMappings = parallelCorpus.VerseMappings.Select(vm=>new Verse(vm.))


            return new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                    TokenizedCorpusId targetTokenizedCorpusId,
                    IEnumerable<Models.VerseMapping> verseMappings,
                    ParallelCorpusId parallelCorpusId)>
                (result: (new TokenizedCorpusId(new Guid()),
                        new TokenizedCorpusId(new Guid()),
                        new List<Models.VerseMapping>() {
                            new Models.VerseMapping(
                                new List<Models.Verse>() {new Models.Verse("MAT", 1, 1)},
                                new List<Models.Verse>() {new Models.Verse("MAT", 1, 1) })},
                        new ParallelCorpusId(new Guid())),
                    success: true,
                    message: "successful result from test");
        }
    }
}
