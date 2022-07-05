using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllTokenizedCorpusIdsByCorpusVersionIdQueryHandler : ProjectDbContextQueryHandler<
        GetAllTokenizedCorpusIdsByCorpusVersionIdQuery,
        RequestResult<IEnumerable<TokenizedCorpusId>>,
        IEnumerable<TokenizedCorpusId>>
    {

        public GetAllTokenizedCorpusIdsByCorpusVersionIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllTokenizedCorpusIdsByCorpusVersionIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<TokenizedCorpusId>>> GetDataAsync(GetAllTokenizedCorpusIdsByCorpusVersionIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: query TokenizedCorpus table by CorpusVersion.Corpus and return enumerable.
            if (request.CorpusVersionId.Id.Equals(new System.Guid("ca761232ed4211cebacd00aa0057b223")))
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
