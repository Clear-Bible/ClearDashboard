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
    public class CreateSemanticDomainMeaningAssociationCommandHandler : ProjectDbContextCommandHandler<CreateSemanticDomainMeaningAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        public CreateSemanticDomainMeaningAssociationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateSemanticDomainMeaningAssociationCommandHandler> logger) : base(
                projectDbContextFactory, 
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(CreateSemanticDomainMeaningAssociationCommand request,
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
                    message: $"SemanticDomain not found for Id '{request.SemanticDomainId.Id}' - unable to create SemanticDomainMeaningAssociation"
                );
            }

            var meaning = ProjectDbContext.Lexicon_Meanings
                .Where(s => s.Id == request.MeaningId.Id)
                .FirstOrDefault();

            if (meaning == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Meaning not found for Id '{request.MeaningId.Id}' - unable to create SemanticDomainMeaningAssociation"
                );
            }

            var semanticDomainMeaningAssociation = new Models.Lexicon_SemanticDomainMeaningAssociation
            {
                SemanticDomainId = semanticDomain.Id,
                MeaningId = meaning.Id
            };

            ProjectDbContext.Lexicon_SemanticDomainMeaningAssociations.Add(semanticDomainMeaningAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}