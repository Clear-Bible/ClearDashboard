using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.BiblicalTerms;
using ParaTextPlugin.Data.Features.Project;
using ParaTextPlugin.Data.Models;

namespace ClearDashboard.WebApiParatextPlugin.Features.BiblicalTerms
{
   
    public class BiblicalTermsController : FeatureSliceController
    {
        public BiblicalTermsController(IMediator mediator, ILogger<BiblicalTermsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<QueryResult<List<BiblicalTermsData>>> GetAsync([FromBody] GetBiblicalTermsByTypeQuery command)
        {
            var result = await ExecuteCommandAsync<QueryResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>(command, CancellationToken.None);
            return result;

        }

    }
}
