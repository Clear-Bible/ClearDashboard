using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.Data.Features.BiblicalTerms;
using MediatR;
using Microsoft.Extensions.Logging;


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
