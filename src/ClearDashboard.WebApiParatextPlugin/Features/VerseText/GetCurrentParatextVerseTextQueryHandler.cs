using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Threading;
using System.Threading.Tasks;
using ProjectType = Paratext.PluginInterfaces.ProjectType;

namespace ClearDashboard.WebApiParatextPlugin.Features.VerseText
{
    public class
        GetParatextVerseTextQueryHandler : IRequestHandler<GetParatextVerseTextQuery, RequestResult<AssignedUser>>
    {
        private readonly IPluginHost _pluginHost;
        private readonly ILogger<GetParatextVerseTextQueryHandler> _logger;
        private readonly MainWindow _mainWindow;
        private readonly IPluginHost _host;
        private readonly IVerseRef _verseRef;

        public GetParatextVerseTextQueryHandler(IPluginHost pluginHost, ILogger<GetParatextVerseTextQueryHandler> logger, 
            IPluginHost host, IVerseRef verseRef, MainWindow mainWindow)
        {
            _pluginHost = pluginHost;
            _logger = logger;
            _mainWindow = mainWindow;
            _host = host;
            _verseRef = verseRef;
        }

        public Task<RequestResult<AssignedUser>> Handle(GetParatextVerseTextQuery request, CancellationToken cancellationToken)
        {
            string verseText = string.Empty;
            if (request.ReturnBackTranslation)
            {
                var projects = _host.GetAllProjects();
                foreach (var project in projects)
                {
                    if (
                        project.Type == ProjectType.BackTranslation &&
                        project.BaseProject != null &&
                        project.BaseProject.ShortName == _mainWindow.Project.ShortName)
                    {
                        verseText = Helpers.VerseText.LookupVerseText(project, request.BookNum, request.ChapterNum, request.VerseNum);
                    }
                }
            }
            else
            {
                verseText = Helpers.VerseText.LookupVerseText(_mainWindow.Project, request.BookNum, request.ChapterNum, request.VerseNum);
            }
            

            var result = new RequestResult<AssignedUser>(new AssignedUser() { Name = verseText });

            if (string.IsNullOrEmpty(verseText))
            {
                result.Data.Name = "Verse Get";
                result.Message = $"No Verse Data in B:{request.BookNum} C:{request.ChapterNum} V:{request.VerseNum} for project {_mainWindow.Project.ShortName}";
                result.Success = false;
            }

            return Task.FromResult(result);
        }
    }
}
