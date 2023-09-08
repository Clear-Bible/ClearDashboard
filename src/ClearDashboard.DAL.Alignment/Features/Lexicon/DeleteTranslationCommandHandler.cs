using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteTranslationCommandHandler : ProjectDbContextCommandHandler<
        DeleteTranslationCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteTranslationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteTranslationCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteTranslationCommand request,
            CancellationToken cancellationToken)
        {
            var translation = ProjectDbContext!.Lexicon_Translations.FirstOrDefault(t => t.Id == request.TranslationId.Id);
            if (translation == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid TranslationId '{request.TranslationId.Id}' found in request"
                );
            }

            foreach (var tr in ProjectDbContext!.Translations
                .Where(e => e.LexiconTranslationId == translation.Id))
            {
                tr.LexiconTranslationId = null;
            }
                
            ProjectDbContext.Remove(translation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}