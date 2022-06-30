using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : AlignmentDbContextQueryHandler<
        GetVersificationAndBookIdByParatextPluginIdQuery,
        RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>,
        (ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {

        public GetVersificationAndBookIdByParatextPluginIdQueryHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, ILogger logger) 
            : base(projectNameDbContextFactory, logger)
        {
        }

        protected override Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>> GetData(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
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
