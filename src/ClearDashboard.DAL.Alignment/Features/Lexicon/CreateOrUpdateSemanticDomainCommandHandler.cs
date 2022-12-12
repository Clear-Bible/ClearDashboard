using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class CreateOrUpdateSemanticDomainCommandHandler : LexiconDbContextCommandHandler<CreateOrUpdateSemanticDomainCommand,
        RequestResult<SemanticDomainId>, SemanticDomainId>
    {
        public CreateOrUpdateSemanticDomainCommandHandler(LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<CreateOrUpdateSemanticDomainCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<SemanticDomainId>> SaveDataAsync(CreateOrUpdateSemanticDomainCommand request,
            CancellationToken cancellationToken)
        {
            Models.SemanticDomain? semanticDomain = null;
            if (request.SemanticDomainId != null)
            {
                semanticDomain = LexiconDbContext!.SemanticDomains.FirstOrDefault(sd => sd.Id == request.SemanticDomainId.Id);
                if (semanticDomain == null)
                {
                    return new RequestResult<SemanticDomainId>
                    (
                        success: false,
                        message: $"Invalid SemanticDomainId '{request.SemanticDomainId.Id}' found in request"
                    );
                }

                semanticDomain.Text = request.Text;
            }
            else
            {
                semanticDomain = new Models.SemanticDomain
                {
                    Id = Guid.NewGuid(),
                    Text = request.Text,
                };
            }

            LexiconDbContext.SemanticDomains.Add(semanticDomain);

            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);
            semanticDomain = LexiconDbContext.SemanticDomains.Include(n => n.User).First(sd => sd.Id == semanticDomain.Id);

            return new RequestResult<SemanticDomainId>(ModelHelper.BuildSemanticDomainId(semanticDomain));
        }
    }
}