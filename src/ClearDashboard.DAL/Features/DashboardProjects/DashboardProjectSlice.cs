using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record GetDashboardProjectQuery : IRequest<RequestResult<ObservableCollection<DashboardProject>>>;

    public class GetDashboardProjectsQueryHandler : ResourceRequestHandler<GetDashboardProjectQuery,
        RequestResult<ObservableCollection<DashboardProject>>, ObservableCollection<DashboardProject>>
    {
        public GetDashboardProjectsQueryHandler(ILogger<GetDashboardProjectsQueryHandler> logger) : base(logger)
        {
        }

       
        protected override string ResourceName { get; set; } = FilePathTemplates.ProjectBaseDirectory;

        public override Task<RequestResult<ObservableCollection<DashboardProject>>> Handle(GetDashboardProjectQuery request, CancellationToken cancellationToken)
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
                foreach (var directoryName in directories)
                {
                    // find the Alignment JSONs
                    var files = Directory.GetFiles(Path.Combine(FilePathTemplates.ProjectBaseDirectory, directoryName), "*.sqlite");
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        var directoryInfo = new DirectoryInfo(directoryName);

                        // add as ListItem
                        var dashboardProject = new DashboardProject
                        {
                            Modified = fileInfo.LastWriteTime,
                            ProjectName = directoryInfo.Name,
                            ShortFilePath = fileInfo.Name,
                            FullFilePath = fileInfo.FullName
                        };

                        // check for user prefs file
                        if (File.Exists(Path.Combine(directoryName, "prefs.jsn")))
                        {
                            // load in the user prefs
                            var userPreferences = UserPreferences.LoadUserPreferencesFile(dashboardProject);

                            // add this to the ProjectViewModel
                            dashboardProject.LastContentWordLevel = userPreferences.LastContentWordLevel;
                            dashboardProject.UserValidationLevel = userPreferences.ValidationLevel;
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
