//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Http;
//using ClearDashboard.WebApiParatextPlugin.Helpers;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Paratext.PluginInterfaces;
//using ParaTextPlugin.Data.Features;
//using ParaTextPlugin.Data.Features.Project;
//using ParaTextPlugin.Data.Models;

//namespace ClearDashboard.WebApiParatextPlugin.Controllers
//{
//    public class ProjectController : ApiController
//    {
//        private readonly IMediator _mediator;
//        private readonly ILogger<ProjectController> _logger;

//        public ProjectController(IMediator mediator, ILogger<ProjectController> logger)
//        { 
//            _mediator = mediator;
//            _logger = logger;
//        }

//        [HttpPost]
//        public async Task<QueryResult<Project>> GetAsync([FromBody]GetCurrentProjectCommand command)
//        {
//            return await _mediator.Send(command);
//        }

//    }
//}
