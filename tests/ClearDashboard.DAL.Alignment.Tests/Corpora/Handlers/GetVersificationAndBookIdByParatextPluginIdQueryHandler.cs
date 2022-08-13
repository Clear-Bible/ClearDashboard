using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : IRequestHandler<
        GetVersificationAndBookIdByParatextProjectIdQuery,
        RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
    {
        public Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
            Handle(GetVersificationAndBookIdByParatextProjectIdQuery command, CancellationToken cancellationToken)
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
