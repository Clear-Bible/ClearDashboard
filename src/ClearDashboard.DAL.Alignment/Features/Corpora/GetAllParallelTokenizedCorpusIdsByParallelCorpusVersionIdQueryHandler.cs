using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler : ProjectDbContextQueryHandler<
        GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery,
        RequestResult<IEnumerable<ParallelTokenizedCorpusId>>,
        IEnumerable<ParallelTokenizedCorpusId>>
    {
        public GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<ParallelTokenizedCorpusId>>> GetDataAsync(GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new RequestResult<IEnumerable<ParallelTokenizedCorpusId>>
                (result: new List<ParallelTokenizedCorpusId>()
                    {
                        new ParallelTokenizedCorpusId(new Guid())
                    },
                    success: true,
                    message: "successful result from test"));
        }
    }
}
