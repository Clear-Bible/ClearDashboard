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


            // get the path to the Paratext projects folder
            var paratextProjectsPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            if (paratextProjectsPath == null)
            {
                throw new Exception($"Paratext projects path is not set");
            }

            var projectName = project.ShortName;

            // get the file path to ProjectUserAccess.xml
            var projectUserAccessFilePath = System.IO.Path.Combine(paratextProjectsPath, projectName, "ProjectUserAccess.xml");
            if (File.Exists(projectUserAccessFilePath) == false)
            {
                return Task.FromResult(result);
            }


            var validUsers = new List<string>();
            try
            {
                // read and parse the xml file
                var xmlStr = File.ReadAllText(projectUserAccessFilePath);
                var str = XElement.Parse(xmlStr);

                var users = str.Elements("User")
                    .ToList();

                foreach (var element in users)
                {
                    var username = element.Attribute("UserName").Value;
                    var role = element.Element("Role").Value;

                    if (role != "None" && role != "Observer" && !string.IsNullOrEmpty(username))
                    {
                        validUsers.Add(username);
                    }
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            result.Data = validUsers;
            return Task.FromResult(result);
        }
    }
}