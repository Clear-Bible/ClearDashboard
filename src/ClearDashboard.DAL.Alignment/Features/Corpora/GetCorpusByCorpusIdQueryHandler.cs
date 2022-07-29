using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetCorpusByCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetCorpusByCorpusIdQuery,
        RequestResult<Corpus>,
        Corpus>
    {
        private readonly IMediator _mediator;

        public GetCorpusByCorpusIdQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetCorpusByCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Corpus>> GetDataAsync(GetCorpusByCorpusIdQuery request, CancellationToken cancellationToken)

        {
            //DB Impl notes: use request.CorpusId to retrieve from Corpus table and return it

            var corpus = ProjectDbContext.Corpa.FirstOrDefault(pc => pc.Id == request.CorpusId.Id);
            if (corpus == null)
            {
                return new RequestResult<Corpus>
                (
                    success: false,
                    message: $"Corpus not found for CorpusId '{request.CorpusId.Id}'"
                );
            }

            return new RequestResult<Corpus>
            (
                new Corpus(
                    request.CorpusId, 
                    _mediator, 
                    corpus.IsRtl, 
                    corpus.Name, 
                    corpus.Language, 
                    corpus.CorpusType)
            );
        }
    }
}
