using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Users
{
    public class GetAllEditableUsersQueryHandler : IRequestHandler<GetAllEditableProjectUsersQuery, RequestResult<List<string>>>
    {
        private readonly IPluginHost _pluginHost;
        private readonly ILogger<GetAllEditableUsersQueryHandler> _logger;
        private readonly IPluginChildWindow _parent;

        public GetAllEditableUsersQueryHandler(IPluginHost pluginHost, ILogger<GetAllEditableUsersQueryHandler> logger, IPluginChildWindow parent)
        {
            _pluginHost = pluginHost;
            _logger = logger;
            _parent = parent;
        }

        public Task<RequestResult<List<string>>> Handle(GetAllEditableProjectUsersQuery request, CancellationToken cancellationToken)
        {
            var project = _parent.CurrentState.Project;

            if (project == null)
            {
                throw new Exception($"project is not set");
            }

            var result = new RequestResult<List<string>>(new List<string>());

            var projectName = project.ShortName;

            var users = new List<string>();
            foreach (var user in project.NonObserverUsers)
            {
                users.Add(user.Name);
            }




            return Task.FromResult(result);
        }
    }
}
