using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTranslationSetModelEntryQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationSetModelEntryQuery,
        RequestResult<Dictionary<string, double>?>,
        Dictionary<string, double>?>
    {

        public GetTranslationSetModelEntryQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationSetModelEntryQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<Dictionary<string, double>?>> GetDataAsync(GetTranslationSetModelEntryQuery request, CancellationToken cancellationToken)
        {
            var translationModelEntry = ProjectDbContext!.TranslationModelEntries
                .Include(tm => tm.TargetTextScores.Where(tts => tts.Text != null))
                .Where(tm => tm.TranslationSetId == request.TranslationSetId.Id)
                .Where(tm => tm.SourceText == request.SourceText)
                .Where(tm => tm.TargetTextScores.Count > 0)
                .FirstOrDefault();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            if (translationModelEntry is not null && translationModelEntry.TargetTextScores.Any())
            {
                return new RequestResult<Dictionary<string, double>?>(
                    translationModelEntry.TargetTextScores
                    .OrderByDescending(tts => tts.Score)
                    .ToDictionary(tts => tts.Text!, tts => tts.Score)
                );
            }

            return new RequestResult<Dictionary<string, double>?>(null);
        }
    }
}
