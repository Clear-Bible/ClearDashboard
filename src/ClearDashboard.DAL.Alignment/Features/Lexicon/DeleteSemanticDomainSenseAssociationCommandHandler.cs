using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainSenseAssociationCommandHandler : ProjectDbContextCommandHandler<
        DeleteSemanticDomainSenseAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainSenseAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteSemanticDomainSenseAssociationCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainSenseAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var association = ProjectDbContext!.Lexicon_SemanticDomainSenseAssociations.FirstOrDefault(sd => 
                sd.SemanticDomainId == request.SemanticDomainId.Id && sd.SenseId == request.SenseId.Id);

            if (association == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId}' / SenseId '{request.SenseId}' combination found in request"
                );
            }

            ProjectDbContext.Remove(association);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}