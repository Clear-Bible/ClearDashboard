using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features.TextCollections
{
    public class TextCollectionsController : FeatureSliceController
    {
        public TextCollectionsController(IMediator mediator, ILogger<TextCollectionsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<TextCollection>>> GetAsync([FromBody] GetTextCollectionsQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<TextCollection>>, List<TextCollection>>(command, CancellationToken.None);
            return result;
        }
    }
}
