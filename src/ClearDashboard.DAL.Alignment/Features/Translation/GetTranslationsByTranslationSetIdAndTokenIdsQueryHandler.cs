using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationsByTranslationSetIdAndTokenIdsQuery,
        RequestResult<IEnumerable<Alignment.Translation.Translation>>,
        IEnumerable<Alignment.Translation.Translation>>
    {

        public GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationsByTranslationSetIdAndTokenIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Alignment.Translation.Translation>>> GetDataAsync(GetTranslationsByTranslationSetIdAndTokenIdsQuery request, CancellationToken cancellationToken)
        {
            //var bookNumbers = request.TokenIds.GroupBy(t => t.BookNumber).Select(grp => grp.Key);
            var engineTokenIds = request.TokenIds.Select(t => t.ToString()).ToList();
            var translations = ProjectDbContext!.Translations
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr => engineTokenIds.Contains(tr.SourceTokenComponent!.EngineTokenId!))
                .Select(t => new Alignment.Translation.Translation(
                    ModelHelper.BuildToken(t.SourceTokenComponent!),
                    t.TargetText ?? string.Empty,
                    t.TranslationState.ToString()));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Alignment.Translation.Translation>>( translations );
        }
     }
}
