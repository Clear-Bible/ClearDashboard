using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.AllProjects
{
    public class GetAllProjectsSlice
    {
        public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, RequestResult<List<ParatextProject>>>
        {
            private readonly IProject _project;
            private readonly ILogger<GetAllProjectsQueryHandler> _logger;
            private readonly IPluginHost _host;
            private readonly IVerseRef _verseRef;
            private readonly MainWindow _mainwindow;

            public GetAllProjectsQueryHandler(IProject project, ILogger<GetAllProjectsQueryHandler> logger,
                IPluginHost host, IVerseRef verseRef, MainWindow mainwindow)
            {
                _project = project;
                _logger = logger;
                _host = host;
                _verseRef = verseRef;
                _mainwindow = mainwindow;
            }

            public Task<RequestResult<List<ParatextProject>>> Handle(GetAllProjectsQuery request,
                CancellationToken cancellationToken)
            {
                var allProjects = _mainwindow.GetAllProjects();
                var result = new RequestResult<List<ParatextProject>>(allProjects);
                return Task.FromResult(result);
            }
        }
    }
}
