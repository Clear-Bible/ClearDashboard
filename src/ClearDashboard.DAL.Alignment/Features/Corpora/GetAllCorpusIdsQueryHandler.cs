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
            var corpusIds = ModelHelper.AddIdIncludesCorpaQuery(ProjectDbContext)
                .Select(c => ModelHelper.BuildCorpusId(c));

            return Task.FromResult(new RequestResult<IEnumerable<CorpusId>>(corpusIds.ToList()));
        }

    }


}
