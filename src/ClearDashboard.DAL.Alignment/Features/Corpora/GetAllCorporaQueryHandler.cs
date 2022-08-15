using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetAllCorporaQueryHandler : ProjectDbContextQueryHandler<
        GetAllCorporaQuery,
        RequestResult<IEnumerable<Corpus>>,
        IEnumerable<Corpus>
        >
    {
        private readonly IMediator _mediator;

        public GetAllCorporaQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllCorpusIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Corpus>>> GetDataAsync(GetAllCorporaQuery request, CancellationToken cancellationToken)
        {
            var corpora = await Task.WhenAll(ProjectDbContext.Corpa.Select(corpus =>
                Corpus.Get(_mediator, new CorpusId(corpus.Id))
            ));

            return new RequestResult<IEnumerable<Corpus>>(corpora);

        }

    }


}
