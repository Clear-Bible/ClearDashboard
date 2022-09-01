using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTranslationsByTranslationSetIdAndTokenIdRangeQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationsByTranslationSetIdAndTokenIdRangeQuery,
        RequestResult<IEnumerable<Alignment.Translation.Translation>>,
        IEnumerable<Alignment.Translation.Translation>>
    {

        public GetTranslationsByTranslationSetIdAndTokenIdRangeQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationsByTranslationSetIdAndTokenIdRangeQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Alignment.Translation.Translation>>> GetDataAsync(GetTranslationsByTranslationSetIdAndTokenIdRangeQuery request, CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext.TranslationSets
                .Include(ts => ts.Translations)
                .Where(ts => ts.Id == request.TranslationSetId.Id)
                .FirstOrDefault();
            if (translationSet == null)
            {
                return new RequestResult<IEnumerable<Alignment.Translation.Translation>>
                (
                    success: false,
                    message: $"TranslationSet not found for TranslationSetId '{request.TranslationSetId.Id}'"
                );
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var translationsDB = translationSet.Translations.AsEnumerable();

            // FIXME:  change this to filter by firstTokenId / lastTokenId range
            // instead of bookId

            //if (request.bookId != null)
            //{
            //    int? bookNumber = FileGetBookIds.BookIds
            //        .Where(x => x.silCannonBookAbbrev == request.bookId)
            //        .Select(x => int.Parse(x.silCannonBookNum))
            //        .FirstOrDefault();
            //    if (bookNumber != null) {
            //        translationsDB = translationsDB.Where(t => t.Token!.BookNumber == bookNumber);
            //    }
            //}

            var translations = translationsDB
                .Select(t => new Alignment.Translation.Translation(
                    new Token(
                        new TokenId(
                            t.SourceToken!.BookNumber,
                            t.SourceToken.ChapterNumber,
                            t.SourceToken.VerseNumber,
                            t.SourceToken.WordNumber,
                            t.SourceToken.SubwordNumber),
                        t.SourceToken.SurfaceText ?? string.Empty,
                        t.SourceToken.TrainingText ?? string.Empty), 
                    t.TargetText ?? string.Empty, 
                    t.TranslationState.ToString()));

            return new RequestResult<IEnumerable<Alignment.Translation.Translation>>( translations );
        }
    }


}
