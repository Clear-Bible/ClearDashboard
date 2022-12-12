using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
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
    public class DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler : ProjectDbContextCommandHandler<
        DeleteSemanticDomainLexicalItemDefinitionAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteSemanticDomainLexicalItemDefinitionAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainLexicalItemDefinitionAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var association = ProjectDbContext!.SemanticDomainLexicalItemDefinitionAssociations.FirstOrDefault(sl => 
                sl.SemanticDomainId == request.SemanticDomainId.Id && sl.LexicalItemDefinitionId == request.LexicalItemDefinitionId.Id);

            if (association == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId}' / LexicalItemDefinitionId '{request.LexicalItemDefinitionId}' combination found in request"
                );
            }

            ProjectDbContext.Remove(association);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}