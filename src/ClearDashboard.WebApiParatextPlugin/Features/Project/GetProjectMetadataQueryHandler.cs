using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class GetProjectMetadataQueryHandler : IRequestHandler<GetProjectMetadataQuery, RequestResult<List<ParatextProjectMetadata>>>
    {
       
        private readonly ILogger<GetProjectMetadataQueryHandler> _logger;
       
        private readonly MainWindow _mainWindow;

        public GetProjectMetadataQueryHandler(ILogger<GetProjectMetadataQueryHandler> logger, MainWindow mainWindow)
        {
           _logger = logger;
           _mainWindow = mainWindow;
        }

        public Task<RequestResult<List<ParatextProjectMetadata>>> Handle(GetProjectMetadataQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var projectMetadata = _mainWindow.GetProjectMetadata();
                var result = new RequestResult<List<ParatextProjectMetadata>>(projectMetadata);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving metadata for the Paratext projects.");
                return Task.FromResult(new RequestResult<List<ParatextProjectMetadata>> { Message = ex.Message, Success = false});
            }
           
        }
    }
}
