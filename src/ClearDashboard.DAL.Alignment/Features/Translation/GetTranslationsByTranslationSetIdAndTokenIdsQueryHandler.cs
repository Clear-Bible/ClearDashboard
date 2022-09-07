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
            var bookNumbers = request.TokenIds.GroupBy(t => t.BookNumber).Select(grp => grp.Key);
            var tokenLocationRefs = request.TokenIds.Select(t => double.Parse(t.ToString())).ToList();
            var translations = ProjectDbContext!.Translations
                .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                .Where(tr => bookNumbers.Contains(tr.SourceToken!.BookNumber))
                .Where(tr => tokenLocationRefs.Contains(
                    double.Parse(ModelHelper.BuildTokenLocationRef(tr.SourceToken!)))
                )
                .Select(tr => new {
                    tr.SourceToken,
                    tr.TargetText,
                    tr.TranslationState,
                    TokenLocationRef = double.Parse(ModelHelper.BuildTokenLocationRef(tr.SourceToken!))
                }) //.AsEnumerable()
//                .Where(tref => tokenLocationRefs.Contains(tref.TokenLocationRef))
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
