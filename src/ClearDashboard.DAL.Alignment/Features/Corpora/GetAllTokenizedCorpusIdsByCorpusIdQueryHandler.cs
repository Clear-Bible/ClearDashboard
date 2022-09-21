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
            
            var tokenizedCorpusIds = ProjectDbContext.TokenizedCorpora
                .Include(tc => tc.User)
                .Where(tc => tc.CorpusId == request.CorpusId.Id)
                .Select(tc => ModelHelper.BuildTokenizedTextCorpusId(tc))
                .AsEnumerable();
            
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
