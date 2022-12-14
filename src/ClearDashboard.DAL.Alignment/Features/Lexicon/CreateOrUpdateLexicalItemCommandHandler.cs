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
    public class CreateOrUpdateLexicalItemCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateLexicalItemCommand,
        RequestResult<LexicalItemId>, LexicalItemId>
    {
        public CreateOrUpdateLexicalItemCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateOrUpdateLexicalItemCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<LexicalItemId>> SaveDataAsync(CreateOrUpdateLexicalItemCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Models.Lexicon_LexicalItem? lexicalItem = null;
                if (request.LexicalItemId != null)
                {
                    lexicalItem = ProjectDbContext!.Lexicon_LexicalItems.FirstOrDefault(li => li.Id == request.LexicalItemId.Id);
                    if (lexicalItem == null)
                    {
                        return new RequestResult<LexicalItemId>
                        (
                            success: false,
                            message: $"Invalid LexicalItemId '{request.LexicalItemId.Id}' found in request"
                        );
                    }

                    lexicalItem.Lemma = request.Lemma;
                    lexicalItem.Language = request.Language;
                }
                else
                {
                    lexicalItem = new Models.Lexicon_LexicalItem
                    {
                        Id = Guid.NewGuid(),
                        Lemma = request.Lemma,
                        Language = request.Language
                    };
                }

                ProjectDbContext.Lexicon_LexicalItems.Add(lexicalItem);

                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
                lexicalItem = ProjectDbContext.Lexicon_LexicalItems.Include(n => n.User).First(li => li.Id == lexicalItem.Id);

                return new RequestResult<LexicalItemId>(ModelHelper.BuildLexicalItemId(lexicalItem));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is not null)
                {
                    return new RequestResult<LexicalItemId>
                    (
                        success: false,
                        message: $"{ex.InnerException.Message}"
                    );
                }
                else
                {
                    return new RequestResult<LexicalItemId>
                    (
                        success: false,
                        message: $"{ex.Message}"
                    );
                }
            }
        }
    }
}