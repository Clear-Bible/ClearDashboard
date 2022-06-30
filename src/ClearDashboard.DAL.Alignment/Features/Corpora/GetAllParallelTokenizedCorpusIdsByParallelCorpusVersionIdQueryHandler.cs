using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQueryHandler : IRequestHandler<
        GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery,
        RequestResult<IEnumerable<ParallelTokenizedCorpusId>>>
    {
        public Task<RequestResult<IEnumerable<ParallelTokenizedCorpusId>>>
            Handle(GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery command, CancellationToken cancellationToken)
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
