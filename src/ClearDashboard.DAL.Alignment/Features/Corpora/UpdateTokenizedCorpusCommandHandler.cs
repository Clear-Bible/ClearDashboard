using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
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
    public class UpdateTokenizedCorpusCommandHandler : ProjectDbContextCommandHandler<UpdateTokenizedCorpusCommand, RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public UpdateTokenizedCorpusCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateTokenizedCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(UpdateTokenizedCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. find tokenizedCorpus based on request.TokenizedCorpusId

            var tokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .FirstOrDefault(e => e.Id == request.TokenizedTextCorpusId.Id);
            if (tokenizedCorpus == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid TokenizedTextCorpusId '{request.TokenizedTextCorpusId.Id}' found in request"
                );
            }

            tokenizedCorpus.DisplayName = request.TokenizedTextCorpusId.DisplayName;
            tokenizedCorpus.Metadata = request.TokenizedTextCorpusId.Metadata;

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}
