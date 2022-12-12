using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSemanticDomainAndAssociationsCommandHandler : LexiconDbContextCommandHandler<
        DeleteSemanticDomainAndAssociationsCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSemanticDomainAndAssociationsCommandHandler(
            LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<DeleteSemanticDomainAndAssociationsCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSemanticDomainAndAssociationsCommand request,
            CancellationToken cancellationToken)
        {
            var semanticDomain = LexiconDbContext!.SemanticDomains.FirstOrDefault(sd => sd.Id == request.SemanticDomainId.Id);
            if (semanticDomain == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SemanticDomainId '{request.SemanticDomainId.Id}' found in request"
                );
            }

            // The data model should be set up to do a cascade delete of
            // any SemanticDomainLexicalItemDefinitionAssociations when
            // the following executes:
            LexiconDbContext.Remove(semanticDomain);
            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}