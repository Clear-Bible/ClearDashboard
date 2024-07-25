using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class GetWordAnalysesQueryController : FeatureSliceController
    {
        public GetWordAnalysesQueryController(IMediator mediator, ILogger<GetWordAnalysesQueryController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>> GetAsync([FromBody] GetWordAnalysesQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>, IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis>>(command, CancellationToken.None);
            return result;
        }
    }
}
