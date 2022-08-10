using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Versification
{
    public class VersificationBooksByParatextIdController : FeatureSliceController
    {
        public VersificationBooksByParatextIdController(IMediator mediator, ILogger<VersificationBooksByParatextIdController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>> 
            GetAsync([FromBody] GetVersificationAndBookIdByParatextPluginIdQuery command)
        {
            var result =
                await ExecuteRequestAsync<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>,
                    (ScrVers? versification, IEnumerable<string> bookAbbreviations)>(command, CancellationToken.None);
            return result;

        }

    }
}
