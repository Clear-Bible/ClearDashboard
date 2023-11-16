using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;


namespace ClearDashboard.WebApiParatextPlugin.Features.BiblicalTerms
{

    public class BiblicalTermsController : FeatureSliceController
    {
        public BiblicalTermsController(IMediator mediator, ILogger<BiblicalTermsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<BiblicalTermsData>>> GetAsync([FromBody] GetBiblicalTermsByTypeQuery command)
        {
            // in 1

            var result = await ExecuteRequestAsync<RequestResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>(command, CancellationToken.None);
            return result;

        }

    }
}
