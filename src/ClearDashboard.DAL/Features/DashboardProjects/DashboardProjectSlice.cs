using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record GetDashboardProjectsCommand : IRequest<RequestResult<ObservableCollection<DashboardProject>>>;

    public class GetDashboardProjectsCommandHandler : ResourceRequestHandler<GetDashboardProjectsCommand,
        RequestResult<ObservableCollection<DashboardProject>>, ObservableCollection<DashboardProject>>
    {
        public GetDashboardProjectsCommandHandler(ILogger<GetDashboardProjectsCommandHandler> logger) : base(logger)
        {
        }

       
        protected override string ResourceName { get; set; } = FilePathTemplates.ProjectBaseDirectory;

        public override Task<RequestResult<ObservableCollection<DashboardProject>>> Handle(GetDashboardProjectsCommand request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<ObservableCollection<DashboardProject>>(new ObservableCollection<DashboardProject>());
            try
            {
                queryResult.Data = ProcessData();
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult, $"An unexpected error occurred while enumerating the {ResourceName} directory for projects", ex);
            }

            return Task.FromResult(queryResult);

        }

        protected override ObservableCollection<DashboardProject> ProcessData()
        {
            var projectList = new ObservableCollection<DashboardProject>();
            // check for Projects subfolder
            var directories = Directory.GetDirectories(ResourceName);

            if (directories.Length == 0)
            {
                //no projects here yet
                return projectList;
            }
            else
            {
                foreach (var dirName in directories)
                {
                    // find the Alignment JSONs
                    var files = Directory.GetFiles(Path.Combine(FilePathTemplates.ProjectBaseDirectory, dirName), "*.sqlite");
                    foreach (var file in files)
                    {
                        var fi = new FileInfo(file);
                        var di = new DirectoryInfo(dirName);

                        // add as ListItem
                        var dashboardProject = new DashboardProject
                        {
                            LastChanged = fi.LastWriteTime,
                            ProjectName = di.Name,
                            ShortFilePath = fi.Name,
                            FullFilePath = fi.FullName
                        };

                        // check for user prefs file
                        if (File.Exists(Path.Combine(dirName, "prefs.jsn")))
                        {
                            // load in the user prefs
                            var up = new UserPrefs();
                            up = up.LoadUserPrefFile(dashboardProject);

                            // add this to the ProjectViewModel
                            dashboardProject.LastContentWordLevel = up.LastContentWordLevel;
                            dashboardProject.UserValidationLevel = up.ValidationLevel;
                        }

                        //dashboardProject.JsonProjectName = GetJsonProjectName(file);
                        //if (dashboardProject.JsonProjectName != "")
                        //{
                        //    dashboardProject.HasJsonProjectName = true;
                        //}

                        projectList.Add(dashboardProject);
                    }
                }
            }

            return projectList;
        }
    }
}
