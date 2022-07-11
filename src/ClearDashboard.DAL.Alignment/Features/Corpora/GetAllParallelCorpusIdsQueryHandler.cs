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
            return Task.FromResult(
                new RequestResult<IEnumerable<ParallelCorpusId>>
                (result: new List<ParallelCorpusId>()
                    {
                        new ParallelCorpusId(new Guid())
                    },
                    success: true,
                    message: "successful result from test"));
        }
    }
}
