using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler : LexiconDbContextCommandHandler<
        DeleteSemanticDomainLexicalItemDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler(
            LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainLexicalItemDefinitionAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var association = LexiconDbContext!.SemanticDomainLexicalItemDefinitionAssociations.FirstOrDefault(sl => 
                sl.SemanticDomainId == request.SemanticDomainId.Id && sl.LexicalItemDefinitionId == request.LexicalItemDefinitionId.Id);

            if (association == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId}' / LexicalItemDefinitionId '{request.LexicalItemDefinitionId}' combination found in request"
                );
            }

            LexiconDbContext.Remove(association);
            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}