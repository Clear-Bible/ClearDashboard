using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.WebApiParatextPlugin.Features.Project;

namespace ClearDashboard.WebApiParatextPlugin.Features.VerseText
{
    public class
        GetCurrentParatextVerseTextQueryHandler : IRequestHandler<GetCurrentParatextVerseTextQuery, RequestResult<AssignedUser>>
    {
        private readonly IPluginHost _pluginHost;
        private readonly ILogger<GetCurrentProjectQueryHandler> _logger;

        public GetCurrentParatextVerseTextQueryHandler(IPluginHost pluginHost, ILogger<GetCurrentProjectQueryHandler> logger)
        {
            _pluginHost = pluginHost;
            _logger = logger;
        }

        public Task<RequestResult<AssignedUser>> Handle(GetCurrentParatextVerseTextQuery request,
            CancellationToken cancellationToken)
        {

            var name = _pluginHost.UserInfo.Name;
            var result = new RequestResult<AssignedUser>(new AssignedUser() { Name = name });

            if (string.IsNullOrEmpty(name))
            {
                result.Data.Name = "USER UNKNOWN";
                result.Message = "There are no users registered with Paratext";
                result.Success = false;
            }
            return Task.FromResult(result);
        }
    }
}
