using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, RequestResult<IReadOnlyList<ExternalNote>>>
    {
        private readonly IPluginHost _host;
        private readonly MainWindow _mainWindow;
        private readonly ILogger<GetNotesQueryHandler> _logger;

        public GetNotesQueryHandler(IPluginHost host, MainWindow mainWindow, ILogger<GetNotesQueryHandler> logger)
        {
            _host = host;
            _mainWindow = mainWindow;
            _logger = logger;
        }
        public Task<RequestResult<IReadOnlyList<ExternalNote>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            if (request.Data.ExternalProjectId.Equals(string.Empty))
            {
                throw new Exception($"externalprojectid is not set");
            }
            IProject project;
            try
            {
                project = _host.GetAllProjects(true)
                    .Where(p => p.ID.Equals(request.Data.ExternalProjectId))
                    .First();
            }
            catch (InvalidOperationException) //empty, which means the external project doesn't exist
            {
                return Task.FromResult(new RequestResult<IReadOnlyList<ExternalNote>>(new List<ExternalNote>()));
            }

            ParatextProjectMetadata paratextProjectMetadata;
            try
            {
                paratextProjectMetadata = _mainWindow.GetProjectMetadata()
                    .Where(pm => pm.Id == request.Data.ExternalProjectId)
                    .SingleOrDefault();

                if (paratextProjectMetadata == null) //means the external project doesn't exist
                {
                    var message = $"Cannot retrieve ExternalLabels for external project id {request.Data.ExternalProjectId} because project id doesn't exist.";
                    _logger.LogError(message);
                    throw new Exception(message);
                }
            }
            catch (InvalidOperationException) //more than one project with the same id
            {
                var message = $"Cannot retrieve ExternalLabels for external project id {request.Data.ExternalProjectId} because more than one project with the same project id found!";
                _logger.LogError(message);
                throw new Exception(message);
            }

            var paratextChapterNotes = project.GetNotes(request.Data.BookNumber, request.Data.ChapterNumber, request.Data.IncludeResolved);

            return Task.FromResult(new RequestResult<IReadOnlyList<ExternalNote>>(paratextChapterNotes
                .Select(ptn => ptn
                    .GetExternalNote(project, _logger)
                    .SetExternalLabelsFromExternalLabelIds(paratextProjectMetadata, _logger))
                .ToList()));
        }
    }
}
