using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
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
    public class PutLexicalItemDefinitionCommandHandler : ProjectDbContextCommandHandler<PutLexicalItemDefinitionCommand,
        RequestResult<LexicalItemDefinitionId>, LexicalItemDefinitionId>
    {
        private readonly IMediator _mediator;

        public PutLexicalItemDefinitionCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutLexicalItemDefinitionCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LexicalItemDefinitionId>> SaveDataAsync(PutLexicalItemDefinitionCommand request,
            CancellationToken cancellationToken)
        {
            Models.LexicalItemDefinition? lexicalItemDefinition = null;
            if (request.LexicalItemDefinition.LexicalItemDefinitionId != null)
            {
                lexicalItemDefinition = ProjectDbContext!.LexicalItemDefinitions.Include(n => n.User).FirstOrDefault(lid => lid.Id == request.LexicalItemDefinition.LexicalItemDefinitionId.Id);
                if (lexicalItemDefinition == null)
                {
                    return new RequestResult<LexicalItemDefinitionId>
                    (
                        success: false,
                        message: $"Invalid LexicalItemDefinitionId '{request.LexicalItemDefinition.LexicalItemDefinitionId.Id}' found in request"
                    );
                }

                lexicalItemDefinition.Text = request.LexicalItemDefinition.Text;
                lexicalItemDefinition.Language = request.LexicalItemDefinition.Language;
            }
            else
            {
                lexicalItemDefinition = new Models.LexicalItemDefinition
                {
                    Id = request.LexicalItemDefinition.LexicalItemDefinitionId?.Id ?? Guid.NewGuid(),
                    Text = request.LexicalItemDefinition.Text,
                    Language = request.LexicalItemDefinition.Language,
                    LexicalItemId = request.LexicalItemId.Id
                };

                ProjectDbContext.LexicalItemDefinitions.Add(lexicalItemDefinition);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            lexicalItemDefinition = ProjectDbContext.LexicalItemDefinitions
                .Include(n => n.User)
                .First(lid => lid.Id == lexicalItemDefinition.Id);

            return new RequestResult<LexicalItemDefinitionId>(ModelHelper.BuildLexicalItemDefinitionId(lexicalItemDefinition));
        }
    }
}