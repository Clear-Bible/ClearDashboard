using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: Screen
    {
        #region   Member Variables

        public ILog _log { get; set; }

        private readonly INavigationService _navigationService;

        #endregion

        #region Observable Objects

        public ObservableCollection<DashboardProject> DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();

        #endregion

        #region Constructor

        public LandingViewModel(ILog log,  INavigationService navigationService)
        {
            // grab a copy of the current logger from the App.xaml.cs
            if (Application.Current is ClearDashboard.Wpf.App)
            {
                //_logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
            }

            _log = log;
      
            _navigationService = navigationService;

          
            // get the clearsuite projects
            DashboardProjects = LoadExistingProjects();

            log.Info("LandingViewModel constructor called.");
        }

        #endregion



        #region Methods

        public void CreateNewProject()
        {
            _log.Info("CreateNewProject called.");
            _navigationService.NavigateToViewModel<CreateNewProjectsViewModel>();
        }

        public void Workspace()
        {
            _log.Info("Workspace called.");
            _navigationService.NavigateToViewModel<WorkSpaceViewModel>();
            
        }

        public void Settings()
        {
            _log.Info("Settings called.");
            _navigationService.NavigateToViewModel<SettingsViewModel>();

        }

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
