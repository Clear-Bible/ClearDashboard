using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
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
            var translations = ProjectDbContext!.Translations
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr =>
                    tr.SourceToken!.BookNumber >= request.FirstTokenId.BookNumber &&
                    tr.SourceToken!.BookNumber <= request.LastTokenId.BookNumber)
                .Select(tr => new {
                    tr.SourceToken,
                    tr.TargetText,
                    tr.TranslationState,
                    TokenLocationRef = double.Parse(ModelHelper.BuildTokenLocationRef(tr.SourceToken!))
                }).AsEnumerable()
                .Where(tref => 
                    tref.TokenLocationRef >= double.Parse(request.FirstTokenId.ToString()) && 
                    tref.TokenLocationRef <= double.Parse(request.LastTokenId.ToString()))
                .Select(t => new Alignment.Translation.Translation(
                    ModelHelper.BuildToken(t.SourceToken!),
                    t.TargetText ?? string.Empty,
                    t.TranslationState.ToString()));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Alignment.Translation.Translation>>( translations );
        }
    }


}
