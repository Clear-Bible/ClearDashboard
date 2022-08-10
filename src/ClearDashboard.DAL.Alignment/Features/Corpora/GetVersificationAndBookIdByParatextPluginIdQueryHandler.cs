using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using SIL.Scripture;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : ProjectDbContextQueryHandler<
        GetVersificationAndBookIdByParatextPluginIdQuery,
        RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>,
        (ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {

        public GetVersificationAndBookIdByParatextPluginIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetVersificationAndBookIdByParatextPluginIdQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>> GetDataAsync(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: extracts the versification and bookAbbreviations (SIL) from the corpus identified by command.ParatextPluginId

            return Task.FromResult(
                new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (ScrVers.Original, new List<string>()),
                    success: true,
                    message: "successful result from test"));
        }
    }
}
