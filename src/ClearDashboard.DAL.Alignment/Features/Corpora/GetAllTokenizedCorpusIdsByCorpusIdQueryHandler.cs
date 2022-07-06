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
            //DB Impl notes: query TokenizedCorpus table by command.CorpusId and return enumerable of all TokenizedCorpus.Id.
            if (request.CorpusId.Id.Equals(new System.Guid("ca761232ed4211cebacd00aa0057b223")))
            {
                return Task.FromResult(
                    new RequestResult<IEnumerable<TokenizedCorpusId>>
                    (result: new List<TokenizedCorpusId>()
                        {
                            new TokenizedCorpusId(new Guid()),
                            new TokenizedCorpusId(new Guid())
                        },
                        success: true,
                        message: "successful result from test"));
            }
            return Task.FromResult(
                new RequestResult<IEnumerable<TokenizedCorpusId>>
                (result: new List<TokenizedCorpusId>(),
                    success: true,
                    message: "successful result from test"));
        }
    }


}
