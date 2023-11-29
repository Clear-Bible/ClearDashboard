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

        private string _fileNameBookNameForm = string.Empty;
        private string _fileNamePrePart = string.Empty;
        private string _fileNamePostPart = string.Empty;


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

            // get the project file pattern from the settings.xml file
            GetProjectSettingsXML(dirPath);

            // get the list of files in the directory to return the file paths
            foreach (var book in project.AvailableBooks)
            {
                // build the file name
                string fileName;

                if (_fileNameBookNameForm == "41MAT")
                    fileName = $"{_fileNamePrePart}{book.Number.ToString().PadLeft(2, '0')}{book.Code}{_fileNamePostPart}";
                else if (_fileNameBookNameForm == "MAT")
                    fileName = $"{_fileNamePrePart}{book.Code}{_fileNamePostPart}";
                else
                    fileName = $"{_fileNamePrePart}{book.Number.ToString().PadLeft(2, '0')}{_fileNamePostPart}";


                // get the file path
                var filePath = Path.Combine(dirPath, fileName);
                // check if the file exists
                if (File.Exists(filePath))
                {
                    // add the file path to the list
                    var entry = new ParatextBook
                    {
                        Available = true,
                        BookId = book.Number.ToString().PadLeft(2, '0'),
                        BookNameShort = book.Code,
                        FilePath = filePath,
                        USFM_Num = book.Number
                    };

                    queryResult.Data.Add(entry);
                }
                else
                {
                    // add an empty string to the list as the file does not exist
                    var entry = new ParatextBook
                    {
                        Available = false,
                        BookId = book.Number.ToString().PadLeft(2, '0'),
                        BookNameShort = book.Code,
                        FilePath = string.Empty,
                        USFM_Num = book.Number
                    };
                    queryResult.Data.Add(entry);
                }
            }

            return Task.FromResult(queryResult);
        }

        private void GetProjectSettingsXML(string projectDirectory)
        {
            var files = new List<string>();

            // get the settings file
            var settingsFile = Path.Combine(projectDirectory, "settings.xml");
            if (File.Exists(settingsFile))
            {
                var xmlStr = File.ReadAllText(settingsFile);
                var str = XElement.Parse(xmlStr);

                // get the data using the <Naming> element
                var prePart = str.Elements("Naming").Attributes("PrePart").FirstOrDefault();
                var postPart = str.Elements("Naming").Attributes("PostPart").FirstOrDefault();
                var bookNameForm = str.Elements("Naming").Attributes("BookNameForm").FirstOrDefault();

                if (bookNameForm is not null)
                {
                    _fileNameBookNameForm = bookNameForm.Value;
                    _fileNamePrePart = prePart.Value;
                    _fileNamePostPart = postPart.Value;
                }
                else
                {
                    // get the data using the other element names
                    _fileNamePostPart = str.Elements("FileNamePostPart").FirstOrDefault().Value;
                    _fileNamePrePart = str.Elements("FileNamePrePart").FirstOrDefault().Value;
                    _fileNameBookNameForm = str.Elements("FileNameBookNameForm").FirstOrDefault().Value;
                }
            }


        }
    }
}
