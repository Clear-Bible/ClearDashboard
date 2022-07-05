using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllParallelCorpusVersionIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllParallelCorpusVersionIdsQuery,
        RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>,
        IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>
    {

        public GetAllParallelCorpusVersionIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllParallelCorpusVersionIdsQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>> GetDataAsync(GetAllParallelCorpusVersionIdsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new RequestResult<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>
                (result: new List<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>()
                    {
                        (new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow), new ParallelCorpusId(new Guid()))
                    },
                    success: true,
                    message: "successful result from test"));
        }
    }
}
