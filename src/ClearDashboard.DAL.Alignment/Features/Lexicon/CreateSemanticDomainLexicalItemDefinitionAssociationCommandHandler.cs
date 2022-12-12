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
    public class CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler : LexiconDbContextCommandHandler<CreateSemanticDomainLexicalItemDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler(
            LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<CreateSemanticDomainLexicalItemDefinitionAssociationCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateSemanticDomainLexicalItemDefinitionAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var semanticDomain = LexiconDbContext.SemanticDomains
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

            var lexicalItemDefinition = LexiconDbContext.LexicalItemDefinitions
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

            LexiconDbContext.SemanticDomainLexicalItemDefinitionAssociations.Add(semanticDomainLexicalItemDefinitionAssociation);
            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}