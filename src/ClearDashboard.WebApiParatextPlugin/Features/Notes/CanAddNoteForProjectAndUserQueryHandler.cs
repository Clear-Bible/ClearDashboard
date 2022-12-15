using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Forms;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using ClearDashboard.WebApiParatextPlugin.Helpers;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class CanAddNoteForProjectAndUserQueryHandler : IRequestHandler<CanAddNoteForProjectAndUserQuery, RequestResult<bool>>
    {
        private readonly IPluginHost _host;
        private readonly IProject _project;
        private readonly MainWindow _mainWindow;
        private readonly IPluginChildWindow _parent;
        private readonly ILogger<AddNoteCommandHandler> _logger;

        public CanAddNoteForProjectAndUserQueryHandler(IPluginHost host, IProject project, MainWindow mainWindow, IPluginChildWindow parent, ILogger<AddNoteCommandHandler> logger)
        {
            _host = host;
            _project = project;
            _mainWindow = mainWindow;
            _parent = parent;
            _logger = logger;
        }
        public Task<RequestResult<bool>> Handle(CanAddNoteForProjectAndUserQuery request, CancellationToken cancellationToken)
        {
            if (request.ParatextProjectId.Equals(string.Empty))
            {
                throw new Exception($"paratextprojectid is not set");
            }
            IProject project;
            try
            {
                project = _host.GetAllProjects(true)
                    .Where(p => p.ID.Equals(request.ParatextProjectId))
                    .First();
            }
            catch (Exception)
            {
                throw new Exception($"paratextprojectid {request.ParatextProjectId} not found");
            }

            //can if project is not a resource and the user is in the list of nonobservers,otherwise can't.
            return Task.FromResult(new RequestResult<bool>(!project.IsResource && project.NonObserverUsers.Select(ui => ui.Name).Contains(request.ParatextUserName)));
        }
    }
}
