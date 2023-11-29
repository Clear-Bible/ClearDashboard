using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using ClearDashboard.WebApiParatextPlugin.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Paratext.PluginInterfaces;
using SIL.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClearDashboard.WebApiParatextPlugin.Features.UsfmFilePath
{
    public class GetUsfmFilePathQueryHandler : IRequestHandler<GetUsfmFilePathQuery, RequestResult<List<ParatextBook>>>
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
        public Task<RequestResult<List<ParatextBook>>> Handle(GetUsfmFilePathQuery request, CancellationToken cancellationToken)
        {
            var paratextId = request.ParatextId;

            var projects = _host.GetAllProjects();
            var project = projects.FirstOrDefault(p => p.ID == paratextId);

            var queryResult = new RequestResult<List<ParatextBook>>(new List<ParatextBook>());

            if (project == null)
            {
                queryResult.Success = false;
                queryResult.Message = $"Paratext Project with ID of {paratextId} was not found";
                return Task.FromResult(queryResult);
            }

            var dirPath = Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName);
            if (Directory.Exists(dirPath) == false)
            {
                queryResult.Success = false;
                queryResult.Message = $"Paratext Project with ID of {paratextId} and path of {dirPath} was not found";
                return Task.FromResult(queryResult);
            }

            var list = ParatextHelpers.GetUsfmFilePaths(dirPath, project);
            queryResult.Data = list;

            return Task.FromResult(queryResult);
        }


    }
}
