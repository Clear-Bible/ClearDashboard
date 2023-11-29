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
    public class GetExternalLabelsQueryHandler : IRequestHandler<GetExternalLabelsQuery, RequestResult<IReadOnlyList<ExternalLabel>>>
    {
        private readonly IPluginHost _host;
        private readonly MainWindow _mainWindow;
        private readonly ILogger<GetExternalLabelsQueryHandler> _logger;

        public GetExternalLabelsQueryHandler(IPluginHost host, MainWindow mainWindow, ILogger<GetExternalLabelsQueryHandler> logger)
        {
            _host = host;
            _mainWindow = mainWindow;
            _logger = logger;
        }
        public Task<RequestResult<IReadOnlyList<ExternalLabel>>> Handle(GetExternalLabelsQuery request, CancellationToken cancellationToken)
        {
            if (request.Data.ExternalProjectId.Equals(string.Empty))
            {
                throw new Exception($"externalprojectid is not set");
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

            return Task.FromResult(new RequestResult<IReadOnlyList<ExternalLabel>>(Extensions.GetExternalLabels(paratextProjectMetadata, _logger)));
        }
    }
}
