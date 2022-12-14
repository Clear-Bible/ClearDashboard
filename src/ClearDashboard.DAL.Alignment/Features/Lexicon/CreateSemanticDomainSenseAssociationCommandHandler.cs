using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class CreateSemanticDomainSenseAssociationCommandHandler : ProjectDbContextCommandHandler<CreateSemanticDomainSenseAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public CreateSemanticDomainSenseAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateSemanticDomainSenseAssociationCommandHandler> logger) : base(
                projectDbContextFactory, 
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateSemanticDomainSenseAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var semanticDomain = ProjectDbContext.Lexicon_SemanticDomains
                .Where(sd => sd.Id == request.SemanticDomainId.Id)
                .FirstOrDefault();

            if (semanticDomain == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"SemanticDomain not found for Id '{request.SemanticDomainId.Id}' - unable to create SemanticDomainSenseAssociation"
                );
            }

            var sense = ProjectDbContext.Lexicon_Senses
                .Where(s => s.Id == request.SenseId.Id)
                .FirstOrDefault();

            if (sense == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Sense not found for Id '{request.SenseId.Id}' - unable to create SemanticDomainSenseAssociation"
                );
            }

            var semanticDomainSenseAssociation = new Models.Lexicon_SemanticDomainSenseAssociation
            {
                SemanticDomainId = semanticDomain.Id,
                SenseId = sense.Id
            };

            ProjectDbContext.Lexicon_SemanticDomainSenseAssociations.Add(semanticDomainSenseAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}