using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.WebApiParatextPlugin.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class AddNoteCommandController : FeatureSliceController
    {
        public AddNoteCommandController(IMediator mediator, ILogger<AllProjectsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<IProjectNote>> GetAsync([FromBody] AddNoteCommand command)
        {
            var result = await ExecuteRequestAsync<RequestResult<IProjectNote>, IProjectNote>(command, CancellationToken.None);
            return result;
        }
    }
}
