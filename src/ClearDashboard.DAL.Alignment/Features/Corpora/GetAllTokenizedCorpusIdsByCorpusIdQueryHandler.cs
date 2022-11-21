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
    public class GetAllTokenizedCorpusIdsByCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetAllTokenizedCorpusIdsByCorpusIdQuery,
        RequestResult<IEnumerable<TokenizedTextCorpusId>>,
        IEnumerable<TokenizedTextCorpusId>>
    {

        public GetAllTokenizedCorpusIdsByCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllTokenizedCorpusIdsByCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<TokenizedTextCorpusId>>> GetDataAsync(GetAllTokenizedCorpusIdsByCorpusIdQuery request, CancellationToken cancellationToken)
        {
            var tokenizedCorpa =
                ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext);

            if (request.CorpusId is not null)
            {
                tokenizedCorpa = tokenizedCorpa
                    .Where(tc => tc.CorpusId == request.CorpusId.Id);
            }

            var tokenizedCorpusIds = tokenizedCorpa
                .Select(tc => ModelHelper.BuildTokenizedTextCorpusId(tc))
                .ToList();
            
            return Task.FromResult(
                new RequestResult<IEnumerable<TokenizedTextCorpusId>>
                    (
                        result: tokenizedCorpusIds,
                        success: true
                    )
                );
        }
    }


}
