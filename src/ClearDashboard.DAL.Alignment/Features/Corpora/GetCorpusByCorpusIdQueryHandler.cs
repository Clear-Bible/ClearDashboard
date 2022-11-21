using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

            var corpus = ProjectDbContext.Corpa
                .Include(c => c.User)
                .FirstOrDefault(pc => pc.Id == request.CorpusId.Id);
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
                    new CorpusId(
                        id: corpus.Id,
                        isRtl: corpus.IsRtl,
                        fontFamily: corpus.FontFamily,
                        name: corpus.Name,
                        displayName: corpus.DisplayName,
                        language: corpus.Language,
                        paratextGuid: corpus.ParatextGuid,
                        corpusType: corpus.CorpusType.ToString(),
                        metadata: corpus.Metadata,
                        created: corpus.Created,
                        userId: ModelHelper.BuildUserId(corpus.User!)
                    )
                )
            );
        }
    }
}
