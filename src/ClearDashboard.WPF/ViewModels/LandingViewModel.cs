using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using MvvmHelpers;
using Serilog;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ObservableObject
    {
        #region   Member Variables

        public ILogger _logger { get; set; }

        #endregion

        #region Observable Objects

        public ObservableCollection<DashboardProject> DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();

        #endregion

        #region Constructor

        public LandingViewModel()
        {
            // grab a copy of the current logger from the App.xaml.cs
            if (Application.Current is ClearDashboard.Wpf.App)
            {
                //_logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
            }

            // get the clearsuite projects
            DashboardProjects = LoadExistingProjects();


        }

        #endregion


        #region Methods

        /// <summary>
        /// Get a listing of all the existing projects in the /Projects
        /// folder below the application
        /// </summary>
        public ObservableCollection<DashboardProject> LoadExistingProjects()
        {

            ObservableCollection<DashboardProject> projectList = new ObservableCollection<DashboardProject>();

            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            appPath = Path.Combine(appPath, "CLEAR_Projects");
            if (!Directory.Exists(appPath))
            {
                // need to create that directory
                try
                {
                    Directory.CreateDirectory(appPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            // check for Projects subfolder
            var directories = Directory.GetDirectories(appPath);

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
                    var files = Directory.GetFiles(Path.Combine(appPath, dirName), "*.json");
                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        DirectoryInfo di = new DirectoryInfo(dirName);

                        // add as ListItem
                        DashboardProject dashboardProject = new DashboardProject
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
                            UserPrefs up = new UserPrefs();
                            up = up.LoadUserPrefFile(dashboardProject);

                            // add this to the ProjectViewModel
                            dashboardProject.LastContentWordLevel = up.LastContentWordLevel;
                            dashboardProject.UserValidationLevel = up.ValidationLevel;
                        }

                        dashboardProject.JsonProjectName = GetJsonProjectName(file);
                        if (dashboardProject.JsonProjectName != "")
                        {
                            dashboardProject.HasJsonProjectName = true;
                        }

                        projectList.Add(dashboardProject);
                    }
                }
            }

            return projectList;
        }

        private string GetJsonProjectName(string filePath)
        {
            string line;
            string project = "";

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                if (line.ToLower().Contains("projectname\":"))
                {
                    project = line.Substring(line.IndexOf(':') + 1);
                    //remove the trailing comma
                    project = project.Substring(0, project.IndexOf(','));
                    // remove the double quotes
                    project = project.Replace("\"", "").Trim();
                    break;
                }
            }
            file.Close();

            return project;
        }


        #endregion // Methods
    }
}
