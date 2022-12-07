using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
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
    public class CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler : ProjectDbContextCommandHandler<CreateSemanticDomainLexicalItemDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider,
            ILogger<CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateSemanticDomainLexicalItemDefinitionAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var semanticDomain = ProjectDbContext.SemanticDomains
                .Where(sd => sd.Id == request.SemanticDomainId.Id)
                .FirstOrDefault();

            if (semanticDomain == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"SemanticDomain not found for Id '{request.SemanticDomainId.Id}' - unable to create SemanticDomainLexicalItemDefinitionAssociation"
                );
            }

            var lexicalItemDefinition = ProjectDbContext.LexicalItemDefinitions
                .Where(lxd => lxd.Id == request.LexicalItemDefinitionId.Id)
                .FirstOrDefault();

            if (lexicalItemDefinition == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"LexicalItemDefinition not found for Id '{request.LexicalItemDefinitionId.Id}' - unable to create SemanticDomainLexicalItemDefinitionAssociation"
                );
            }

            var semanticDomainLexicalItemDefinitionAssociation = new Models.SemanticDomainLexicalItemDefinitionAssociation
            {
                SemanticDomainId = semanticDomain.Id,
                LexicalItemDefinitionId = lexicalItemDefinition.Id
            };

            ProjectDbContext.SemanticDomainLexicalItemDefinitionAssociations.Add(semanticDomainLexicalItemDefinitionAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>();
        }
    }
}