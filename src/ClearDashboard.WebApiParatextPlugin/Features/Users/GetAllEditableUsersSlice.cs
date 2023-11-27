using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClearDashboard.WebApiParatextPlugin.Features.Users
{

    /// <summary>
    /// Get the list of all users that have edit access to the project
    /// </summary>
    public class GetAllEditableProjectUsersQueryHandler :
        IRequestHandler<GetAllEditableProjectUsersQuery, RequestResult<List<string>>>
    {
        private readonly IPluginHost _pluginHost;
        private readonly ILogger<GetAllEditableProjectUsersQueryHandler> _logger;
        private readonly IPluginChildWindow _parent;

        public GetAllEditableProjectUsersQueryHandler(IPluginHost pluginHost, ILogger<GetAllEditableProjectUsersQueryHandler> logger, IPluginChildWindow parent)
        {
            _pluginHost = pluginHost;
            _logger = logger;
            _parent = parent;
        }
        public Task<RequestResult<List<string>>> Handle(GetAllEditableProjectUsersQuery request, CancellationToken cancellationToken)
        {
            var result = new RequestResult<List<string>>(new List<string>());

            // get all the projects
            var projects = _pluginHost.GetAllProjects(false);

            // get the project with the extended notes
            var project = projects.FirstOrDefault(p => p.ID == request.ParatextProjectId);
            if (project == null)
            {
                throw new Exception($"Paratext project with id {request.ParatextProjectId} not found");
            }


            // return empty list if the project is not a translation project    
            var users = new List<string>(project.NonObserverUsers.Select(x => x.Name).OrderBy(x => x));
            result.Data = users;
            return Task.FromResult(result);
        }
    }
}