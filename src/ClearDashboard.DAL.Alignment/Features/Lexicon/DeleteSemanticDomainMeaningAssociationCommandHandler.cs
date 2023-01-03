using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainMeaningAssociationCommandHandler : ProjectDbContextCommandHandler<
        DeleteSemanticDomainMeaningAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainMeaningAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteSemanticDomainMeaningAssociationCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainMeaningAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var association = ProjectDbContext!.Lexicon_SemanticDomainMeaningAssociations.FirstOrDefault(sd => 
                sd.SemanticDomainId == request.SemanticDomainId.Id && sd.MeaningId == request.MeaningId.Id);

            if (association == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId}' / MeaningId '{request.MeaningId}' combination found in request"
                );
            }

            ProjectDbContext.Remove(association);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}