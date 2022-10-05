using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllParallelCorpusIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllParallelCorpusIdsQuery,
        RequestResult<IEnumerable<ParallelCorpusId>>,
        IEnumerable<ParallelCorpusId>>
    {

        public GetAllParallelCorpusIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllParallelCorpusIdsQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<ParallelCorpusId>>> GetDataAsync(GetAllParallelCorpusIdsQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: query ParallelCorpus table and return all ids
            var parallelCorpusIds = ProjectDbContext.ParallelCorpa
                .Include(pc => pc.SourceTokenizedCorpus)
                .Include(pc => pc.TargetTokenizedCorpus)
                .Include(pc => pc.User)
                .Select(pc => ModelHelper.BuildParallelCorpusId(pc));

            return Task.FromResult(new RequestResult<IEnumerable<ParallelCorpusId>>(parallelCorpusIds.ToList()));
       }
    }
}
