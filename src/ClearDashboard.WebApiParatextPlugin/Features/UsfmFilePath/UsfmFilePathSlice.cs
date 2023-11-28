using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;
using ClearDashboard.WebApiParatextPlugin.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.UsfmFilePath
{
    public class GetUsfmFilePathQueryHandler : IRequestHandler<GetUsfmFilePathQuery, RequestResult<List<string>>>
    {
        private readonly IPluginHost _host;
        private readonly IProject _project;
        private readonly ILogger<GetUsfmFilePathQueryHandler> _logger;

        public GetUsfmFilePathQueryHandler(IPluginHost host, IProject project, ILogger<GetUsfmFilePathQueryHandler> logger)
        {
            _host = host;
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<List<string>>> Handle(GetUsfmFilePathQuery request, CancellationToken cancellationToken)
        {
            var paratextId = request.ParatextId;

            var projects = _host.GetAllProjects();
            var project = projects.FirstOrDefault(p => p.ID == paratextId);

            var queryResult = new RequestResult<List<string>>(new List<string>());
            if (project == null)
            {
                queryResult.Success = false;
                queryResult.Message = $"Paratext Project with ID of {paratextId} was not found";
                return Task.FromResult(queryResult);
            }

            
            try
            {
                queryResult.Data = new List<string> { "GEN", "EXO"};
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }

    }
}
