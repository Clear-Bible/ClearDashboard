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
    public class CreateOrUpdateLexicalItemCommandHandler : LexiconDbContextCommandHandler<CreateOrUpdateLexicalItemCommand,
        RequestResult<LexicalItemId>, LexicalItemId>
    {
        public CreateOrUpdateLexicalItemCommandHandler(LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<CreateOrUpdateLexicalItemCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<LexicalItemId>> SaveDataAsync(CreateOrUpdateLexicalItemCommand request,
            CancellationToken cancellationToken)
        {
            Models.LexicalItem? lexicalItem = null;
            if (request.LexicalItemId != null)
            {
                lexicalItem = LexiconDbContext!.LexicalItems.FirstOrDefault(li => li.Id == request.LexicalItemId.Id);
                if (lexicalItem == null)
                {
                    return new RequestResult<LexicalItemId>
                    (
                        success: false,
                        message: $"Invalid LexicalItemId '{request.LexicalItemId.Id}' found in request"
                    );
                }

                lexicalItem.TrainingText = request.TrainingText;
                lexicalItem.Language = request.Language;
            }
            else
            {
                lexicalItem = new Models.LexicalItem
                {
                    Id = Guid.NewGuid(),
                    TrainingText = request.TrainingText,
                    Language = request.Language
                };
            }

            LexiconDbContext.LexicalItems.Add(lexicalItem);

            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);
            lexicalItem = LexiconDbContext.LexicalItems.Include(n => n.User).First(li => li.Id == lexicalItem.Id);

            return new RequestResult<LexicalItemId>(ModelHelper.BuildLexicalItemId(lexicalItem));
        }
    }
}