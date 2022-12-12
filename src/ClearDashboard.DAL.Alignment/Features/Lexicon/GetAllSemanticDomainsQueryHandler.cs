using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetAllSemanticDomainsQueryHandler : ProjectDbContextQueryHandler<
        GetAllSemanticDomainsQuery,
        RequestResult<IEnumerable<SemanticDomain>>,
        IEnumerable<SemanticDomain>>
    {
        public GetAllSemanticDomainsQueryHandler( 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllSemanticDomainsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<SemanticDomain>>> GetDataAsync(GetAllSemanticDomainsQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var semanticDomains = ProjectDbContext.SemanticDomains
                .Include(sd => sd.User)
                .Select(sd => new SemanticDomain(
                    ModelHelper.BuildSemanticDomainId(sd),
                    sd.Text ?? string.Empty
                ));

            return new RequestResult<IEnumerable<SemanticDomain>>
            (
                semanticDomains.ToList()
            );
        }
    }
}
