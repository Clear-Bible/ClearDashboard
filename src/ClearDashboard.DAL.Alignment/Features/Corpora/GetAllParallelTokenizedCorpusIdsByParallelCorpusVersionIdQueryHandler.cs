using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler : AlignmentDbContextQueryHandler<
        GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery,
        RequestResult<IEnumerable<ParallelTokenizedCorpusId>>,
        IEnumerable<ParallelTokenizedCorpusId>>
    {
        public GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, ILogger logger) 
            : base(projectNameDbContextFactory, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<ParallelTokenizedCorpusId>>> GetData(GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery request,
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
