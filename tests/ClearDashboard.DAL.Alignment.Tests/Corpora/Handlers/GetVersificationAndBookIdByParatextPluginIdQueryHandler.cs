using MediatR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

using ClearDashboard.DAL.CQRS;
using ClearBible.Alignment.DataServices.Features.Corpora;

using SIL.Scripture;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : IRequestHandler<
        GetVersificationAndBookIdByParatextPluginIdQuery,
        RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
    {
        public Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
            Handle(GetVersificationAndBookIdByParatextPluginIdQuery command, CancellationToken cancellationToken)
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
