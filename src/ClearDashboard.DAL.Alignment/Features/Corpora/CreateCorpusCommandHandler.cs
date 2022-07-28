using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateCorpusCommandHandler : ProjectDbContextCommandHandler<
        CreateCorpusCommand,
        RequestResult<CorpusId>,
        CorpusId>
    {
        private readonly IMediator _mediator;

        public CreateCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateCorpusCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<CorpusId>> SaveDataAsync(
            CreateCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            // 1. creates a new Corpus

            var corpus = new Corpus
            {
                IsRtl = request.IsRtl,
                Name = request.Name,
                Language = request.Language,
            };

            if (Enum.TryParse<CorpusType>(request.CorpusType, out CorpusType corpusType))
            {
                corpus.CorpusType = corpusType;
            } 
            else
            {
                corpus.CorpusType = CorpusType.Unknown;
            }

            ProjectDbContext.Corpa.Add(corpus);

            // NB:  passing in the cancellation token to SaveChangesAsync.
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            var corpusId = new CorpusId(corpus.Id);

            return new RequestResult<CorpusId>(corpusId);
        }
    }
}