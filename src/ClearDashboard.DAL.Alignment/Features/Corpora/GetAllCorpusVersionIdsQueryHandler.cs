using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllCorpusVersionIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllCorpusVersionIdsQuery,
        RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>,
        IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>
    {

        public GetAllCorpusVersionIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllCorpusVersionIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>> GetDataAsync(GetAllCorpusVersionIdsQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: query CorpusVersion table and return all ids

            return Task.FromResult(
                new RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>
                (result: new List<(CorpusVersionId corpusVersionId, CorpusId corpusId)>() {
                        (new CorpusVersionId(new Guid("ca761232ed4211cebacd00aa0057b223"), DateTime.UtcNow), new CorpusId(new Guid())),
                        (new CorpusVersionId(new Guid("ca761232ed4211cebacd00aa0057b255"), DateTime.UtcNow), new CorpusId(new Guid()))
                    },
                    success: true,
                    message: "successful result from test"));
        }

    }


}
