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
    public class GetAllTokenizedCorpusIdsByCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetAllTokenizedCorpusIdsByCorpusIdQuery,
        RequestResult<IEnumerable<TokenizedCorpusId>>,
        IEnumerable<TokenizedCorpusId>>
    {

        public GetAllTokenizedCorpusIdsByCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllTokenizedCorpusIdsByCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<TokenizedCorpusId>>> GetDataAsync(GetAllTokenizedCorpusIdsByCorpusIdQuery request, CancellationToken cancellationToken)
        {
            
            var tokenizedCorpusIds = ProjectDbContext.TokenizedCorpora
                .Where(tc => tc.CorpusId == request.CorpusId.Id)
                .Select(tc => new TokenizedCorpusId(tc.Id)).AsEnumerable();
            
            return Task.FromResult(
                new RequestResult<IEnumerable<TokenizedCorpusId>>
                    (
                        result: tokenizedCorpusIds,
                        success: true
                    )
                );
        }
    }


}
