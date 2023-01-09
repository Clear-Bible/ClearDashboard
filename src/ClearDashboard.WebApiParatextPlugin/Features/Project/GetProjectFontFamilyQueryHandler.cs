using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class GetProjectFontFamilyQueryHandler : IRequestHandler<GetProjectFontFamilyQuery, RequestResult<string>>
    {
        private const string DefaultFontFamily = "Segoe UI";
        private readonly ILogger<GetProjectFontFamilyQueryHandler> _logger;

        private readonly MainWindow _mainWindow;

        public GetProjectFontFamilyQueryHandler(IProject project, ILogger<GetProjectFontFamilyQueryHandler> logger,
            IPluginHost host, IVerseRef verseRef, MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }

        public Task<RequestResult<string>> Handle(GetProjectFontFamilyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var projectMetadata = _mainWindow.GetProjectMetadata();
                var project = projectMetadata.FirstOrDefault(p => p.Id == request.ParatextProjectId);
                var fontFamily = DefaultFontFamily;
                if (project != null)
                {
                    fontFamily = project.FontFamily;
                }
                var result = new RequestResult<string>(fontFamily);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while retrieving the font family for the Paratext project - '{request.ParatextProjectId}'.");
                return Task.FromResult(new RequestResult<string> { Message = ex.Message, Success = false });
            }

        }
    }
}