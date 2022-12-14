using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System.Threading;

//USE TO ACCESS Vocabulary
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class CreateSemanticDomainDefinitionAssociationCommandHandler : ProjectDbContextCommandHandler<CreateSemanticDomainDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public CreateSemanticDomainDefinitionAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateSemanticDomainDefinitionAssociationCommandHandler> logger) : base(
                projectDbContextFactory, 
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateSemanticDomainDefinitionAssociationCommand request,
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
                    message: $"SemanticDomain not found for Id '{request.SemanticDomainId.Id}' - unable to create SemanticDomainDefinitionAssociation"
                );
            }

            var definition = ProjectDbContext.Lexicon_Definitions
                .Where(d => d.Id == request.DefinitionId.Id)
                .FirstOrDefault();

            if (definition == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Definition not found for Id '{request.DefinitionId.Id}' - unable to create SemanticDomainDefinitionAssociation"
                );
            }

            var semanticDomainDefinitionAssociation = new Models.Lexicon_SemanticDomainDefinitionAssociation
            {
                SemanticDomainId = semanticDomain.Id,
                DefinitionId = definition.Id
            };

            ProjectDbContext.Lexicon_SemanticDomainDefinitionAssociations.Add(semanticDomainDefinitionAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}