using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;


namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class GetCurrentProjectQueryHandler : IRequestHandler<GetCurrentProjectQuery, RequestResult<DataAccessLayer.Models.ParatextProject>>
    {
        private readonly IProject _project;
        private readonly ILogger<GetCurrentProjectQueryHandler> _logger;

        public GetCurrentProjectQueryHandler(IProject project, ILogger<GetCurrentProjectQueryHandler> logger)
        {
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<DataAccessLayer.Models.ParatextProject>> Handle(GetCurrentProjectQuery request, CancellationToken cancellationToken)
        {
            var project =  ConvertIProjectToParatextProject.BuildParatextProjectFromIProject(_project);
            var result = new RequestResult<DataAccessLayer.Models.ParatextProject>(project);
            return Task.FromResult(result);
        }
    }
}
