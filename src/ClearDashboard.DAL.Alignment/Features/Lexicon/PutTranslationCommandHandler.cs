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
    public class PutTranslationCommandHandler : ProjectDbContextCommandHandler<PutTranslationCommand,
        RequestResult<TranslationId>, TranslationId>
    {
        public PutTranslationCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<PutTranslationCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<TranslationId>> SaveDataAsync(PutTranslationCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_Translation? translation = null;
            if (request.Translation.TranslationId.Created != null)
            {
                translation = ProjectDbContext!.Lexicon_Translations.Include(t => t.User).FirstOrDefault(t => t.Id == request.Translation.TranslationId.Id);
                if (translation == null)
                {
                    return new RequestResult<TranslationId>
                    (
                        success: false,
                        message: $"Invalid TranslationId '{request.Translation.TranslationId.Id}' found in request"
                    );
                }

                translation.Text = request.Translation.Text;
            }
            else
            {
                translation = new Models.Lexicon_Translation
                {
                    Id = request.Translation.TranslationId.Id,
                    Text = request.Translation.Text,
                    MeaningId = request.MeaningId.Id
                };

                ProjectDbContext.Lexicon_Translations.Add(translation);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            translation = ProjectDbContext.Lexicon_Translations
                .Include(t => t.User)
                .First(t => t.Id == translation.Id);

            return new RequestResult<TranslationId>(ModelHelper.BuildTranslationId(translation));
        }
    }
}