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
    public class GetAllCorpusIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllCorpusIdsQuery,
        RequestResult<IEnumerable<CorpusId>>,
        IEnumerable<CorpusId>>
    {

        public GetAllCorpusIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllCorpusIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<CorpusId>>> GetDataAsync(GetAllCorpusIdsQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: query Corpus table and return all ids

            return Task.FromResult(
                new RequestResult<IEnumerable<CorpusId>>
                (result: new List<CorpusId>() {
                        new CorpusId(new Guid()),
                        new CorpusId(new Guid())
                    },
                    success: true,
                    message: "successful result from test"));
        }

    }


}
