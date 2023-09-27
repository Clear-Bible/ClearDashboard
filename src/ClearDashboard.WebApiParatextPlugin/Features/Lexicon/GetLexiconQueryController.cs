using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Lexicon
{
    public class GetLexiconQueryController : FeatureSliceController
    {
        public GetLexiconQueryController(IMediator mediator, ILogger<GetLexiconQueryController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>> GetAsync([FromBody] GetLexiconQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>, DataAccessLayer.Models.Lexicon_Lexicon>(command, CancellationToken.None);
            return result;
        }
    }
}
