using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainDefinitionAssociationCommandHandler : ProjectDbContextCommandHandler<
        DeleteSemanticDomainDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainDefinitionAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteSemanticDomainDefinitionAssociationCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainDefinitionAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var association = ProjectDbContext!.Lexicon_SemanticDomainDefinitionAssociations.FirstOrDefault(sd => 
                sd.SemanticDomainId == request.SemanticDomainId.Id && sd.DefinitionId == request.DefinitionId.Id);

            if (association == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId}' / DefinitionId '{request.DefinitionId}' combination found in request"
                );
            }

            ProjectDbContext.Remove(association);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}