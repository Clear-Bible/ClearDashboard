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
    public class PutLexicalItemDefinitionCommandHandler : LexiconDbContextCommandHandler<PutLexicalItemDefinitionCommand,
        RequestResult<LexicalItemDefinitionId>, LexicalItemDefinitionId>
    {
        public PutLexicalItemDefinitionCommandHandler(
            LexiconDbContextFactory? lexiconDbContextFactory,
            ILogger<PutLexicalItemDefinitionCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<LexicalItemDefinitionId>> SaveDataAsync(PutLexicalItemDefinitionCommand request,
            CancellationToken cancellationToken)
        {
            Models.LexicalItemDefinition? lexicalItemDefinition = null;
            if (request.LexicalItemDefinition.LexicalItemDefinitionId != null)
            {
                lexicalItemDefinition = LexiconDbContext!.LexicalItemDefinitions.Include(n => n.User).FirstOrDefault(lid => lid.Id == request.LexicalItemDefinition.LexicalItemDefinitionId.Id);
                if (lexicalItemDefinition == null)
                {
                    return new RequestResult<LexicalItemDefinitionId>
                    (
                        success: false,
                        message: $"Invalid LexicalItemDefinitionId '{request.LexicalItemDefinition.LexicalItemDefinitionId.Id}' found in request"
                    );
                }

                lexicalItemDefinition.TrainingText = request.LexicalItemDefinition.TrainingText;
                lexicalItemDefinition.Language = request.LexicalItemDefinition.Language;
            }
            else
            {
                lexicalItemDefinition = new Models.LexicalItemDefinition
                {
                    Id = request.LexicalItemDefinition.LexicalItemDefinitionId?.Id ?? Guid.NewGuid(),
                    TrainingText = request.LexicalItemDefinition.TrainingText,
                    Language = request.LexicalItemDefinition.Language,
                    LexicalItemId = request.LexicalItemId.Id
                };

                LexiconDbContext.LexicalItemDefinitions.Add(lexicalItemDefinition);
            }

            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);
            lexicalItemDefinition = LexiconDbContext.LexicalItemDefinitions
                .Include(n => n.User)
                .First(lid => lid.Id == lexicalItemDefinition.Id);

            return new RequestResult<LexicalItemDefinitionId>(ModelHelper.BuildLexicalItemDefinitionId(lexicalItemDefinition));
        }
    }
}