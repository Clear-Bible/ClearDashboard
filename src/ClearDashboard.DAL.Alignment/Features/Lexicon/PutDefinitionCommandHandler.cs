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
    public class PutDefinitionCommandHandler : ProjectDbContextCommandHandler<PutDefinitionCommand,
        RequestResult<DefinitionId>, DefinitionId>
    {
        public PutDefinitionCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<PutDefinitionCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<DefinitionId>> SaveDataAsync(PutDefinitionCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_Definition? definition = null;
            if (request.Definition.DefinitionId != null)
            {
                definition = ProjectDbContext!.Lexicon_Definitions.Include(d => d.User).FirstOrDefault(d => d.Id == request.Definition.DefinitionId.Id);
                if (definition == null)
                {
                    return new RequestResult<DefinitionId>
                    (
                        success: false,
                        message: $"Invalid DefinitionId '{request.Definition.DefinitionId.Id}' found in request"
                    );
                }

                definition.Text = request.Definition.Text;
                definition.Language = request.Definition.Language;
            }
            else
            {
                definition = new Models.Lexicon_Definition
                {
                    Id = request.Definition.DefinitionId?.Id ?? Guid.NewGuid(),
                    Text = request.Definition.Text,
                    Language = request.Definition.Language,
                    LexemeId = request.LexemeId.Id
                };

                ProjectDbContext.Lexicon_Definitions.Add(definition);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            definition = ProjectDbContext.Lexicon_Definitions
                .Include(d => d.User)
                .First(d => d.Id == definition.Id);

            return new RequestResult<DefinitionId>(ModelHelper.BuildDefinitionId(definition));
        }
    }
}