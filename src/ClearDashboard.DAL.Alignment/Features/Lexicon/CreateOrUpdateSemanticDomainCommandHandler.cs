using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Vocabulary
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class CreateOrUpdateSemanticDomainCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateSemanticDomainCommand,
        RequestResult<SemanticDomainId>, SemanticDomainId>
    {
        public CreateOrUpdateSemanticDomainCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateOrUpdateSemanticDomainCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<SemanticDomainId>> SaveDataAsync(CreateOrUpdateSemanticDomainCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_SemanticDomain? semanticDomain = null;
            if (request.SemanticDomainId != null)
            {
                semanticDomain = ProjectDbContext!.Lexicon_SemanticDomains.FirstOrDefault(sd => sd.Id == request.SemanticDomainId.Id);
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
                semanticDomain = new Models.Lexicon_SemanticDomain
                {
                    Id = Guid.NewGuid(),
                    Text = request.Text,
                };
            }

            ProjectDbContext.Lexicon_SemanticDomains.Add(semanticDomain);

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            semanticDomain = ProjectDbContext.Lexicon_SemanticDomains.Include(sd => sd.User).First(sd => sd.Id == semanticDomain.Id);

            return new RequestResult<SemanticDomainId>(ModelHelper.BuildSemanticDomainId(semanticDomain));
        }
    }
}