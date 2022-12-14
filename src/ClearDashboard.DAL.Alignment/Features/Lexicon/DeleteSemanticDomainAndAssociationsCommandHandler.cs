using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainAndAssociationsCommandHandler : ProjectDbContextCommandHandler<
        DeleteSemanticDomainAndAssociationsCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainAndAssociationsCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteSemanticDomainAndAssociationsCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainAndAssociationsCommand request,
            CancellationToken cancellationToken)
        {
            var semanticDomain = ProjectDbContext!.Lexicon_SemanticDomains.FirstOrDefault(sd => sd.Id == request.SemanticDomainId.Id);
            if (semanticDomain == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId.Id}' found in request"
                );
            }

            // The data model should be set up to do a cascade delete of
            // any SemanticDomainSenseAssociations when
            // the following executes:
            ProjectDbContext.Remove(semanticDomain);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}