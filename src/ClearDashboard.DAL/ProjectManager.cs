using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Context;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Utility;
using ClearDashboard.DataAccessLayer.ViewModels;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer
{
    public class ProjectManager : IDisposable
    {
        #region Properties

        private readonly ILogger _logger;
        private readonly NamedPipesClient _namedPipesClient;
        private readonly ProjectNameDbContextFactory _projectNameDbContextFactory;

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } = new ObservableRangeCollection<ParatextProjectViewModel>();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } = new ObservableRangeCollection<ParatextProjectViewModel>();

        
        public bool ParatextVisible = false;

        public bool IsPipeConnected { get; set; }

        #region Startup

        public ProjectManager(NamedPipesClient namedPipeClient, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory)
        {
            _logger = logger;
            _projectNameDbContextFactory = projectNameDbContextFactory;
            _logger.LogInformation("'ProjectManager' ctor called.");

            _namedPipesClient = namedPipeClient;
            _namedPipesClient.NamedPipeChanged += HandleNamedPipeChanged;
        }

        #endregion

        public enum PipeAction
        {
            OnConnected,
            OnDisconnected,
            SendText,
            GetBiblicalTermsAll,
            GetBiblicalTermsProject,
            GetSourceVerses,
            GetTargetVerses,
            GetNotes,
            GetProject,
            GetCurrentVerse,
        }


        #endregion

        #region Events

        // event handler to be raised when the Paratext Username changes
        public event EventHandler ParatextUserNameEventHandler;
        public event NamedPipesClient.PipesEventHandler NamedPipeChanged;
        public string ParatextUserName { get; set; } = "";

        private void RaisePipesChangedEvent(PipeMessage pm)
        {
            var args = new PipeEventArgs(pm);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Shutdown

        public void OnClosing()
        {
            IsPipeConnected = false;
            _namedPipesClient.Dispose();
        }

        #endregion

        #region Methods

        private void HandleNamedPipeChanged(object sender, PipeEventArgs args)
        {
            var pm = args.PM;

            if (pm.Action == ActionType.OnConnected)
            {
                this.IsPipeConnected = true;
            } 
            else if (pm.Action == ActionType.OnDisconnected)
            {
                this.IsPipeConnected= false;
            }

            RaisePipesChangedEvent(pm);
        }

        /// <summary>
        /// Get a listing of all the existing projects in the /Projects
        /// folder below the application
        /// </summary>
        public ObservableCollection<DashboardProject> LoadExistingProjects()
        {

            var projectList = new ObservableCollection<DashboardProject>();

            //var appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects");
            if (!Directory.Exists(FilePathTemplates.ProjectBaseDirectory))
            {
                // need to create that directory
                try
                {
                    Directory.CreateDirectory(FilePathTemplates.ProjectBaseDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            // check for Projects subfolder
            var directories = Directory.GetDirectories(FilePathTemplates.ProjectBaseDirectory);

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


        /// <summary>
        /// Taken from LandingViewModel - most likely will be deprecated
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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

        public async Task SetupParatext()
        {
            // detect if Paratext is installed
            var paratextUtils = new ParatextUtils();
            ParatextVisible = paratextUtils.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                var projects = await paratextUtils.GetParatextProjectsOrResources(ParatextUtils.FolderType.Projects);
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var project in projects)
                    {
                        ParatextProjects.Add(TinyMapper.Map<ParatextProjectViewModel>(project));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while initializing");
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                var resources = paratextUtils.GetParatextResources();
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var resource in resources)
                    {
                        ParatextResources.Add(TinyMapper.Map<ParatextProjectViewModel>(resource));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while initializing");
                }
            }
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project's pm directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            var paratextUtils = new ParatextUtils();
            var user = paratextUtils.GetCurrentParatextUser();

            ParatextUserName = user;

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
        }

        public DashboardProject CurrentDashboardProject { get; private set; }

        public DashboardProject CreateDashboardProject()
        {
            CurrentDashboardProject = new DashboardProject
            {
                ParatextUser = ParatextUserName,
                CreationDate = DateTime.Now
            };

            return CurrentDashboardProject;
        }

        public async Task SendPipeMessage(PipeAction action, string text = "")
        {
            var message = new PipeMessage();
            switch (action)
            {
                case PipeAction.OnConnected:
                    message.Action = ActionType.OnConnected;
                    break;
                case PipeAction.OnDisconnected:
                    message.Action = ActionType.OnDisconnected;
                    break;
                case PipeAction.GetCurrentVerse:
                    message.Action = ActionType.GetCurrentVerse;
                    break;
                case PipeAction.SendText:
                    message.Action = ActionType.SendText;
                    message.Text = text;
                    break;
                case PipeAction.GetBiblicalTermsAll:
                    message.Action = ActionType.GetBibilicalTermsAll;
                    break;
                case PipeAction.GetBiblicalTermsProject:
                    message.Action = ActionType.GetBibilicalTermsProject;
                    break;
                case PipeAction.GetSourceVerses:
                    message.Action = ActionType.GetSourceVerses;
                    break;
                case PipeAction.GetTargetVerses:
                    message.Action= ActionType.GetTargetVerses;
                    break;
                case PipeAction.GetNotes:
                    message.Action = ActionType.GetNotes;
                    break;
                case PipeAction.GetProject:
                    message.Action = ActionType.GetProject;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            await _namedPipesClient.WriteAsync(message);
        }

        #endregion


        public async Task CreateNewProject(DashboardProject dashboardProject)
        {
            var projectAssets = await _projectNameDbContextFactory.Create(dashboardProject.ProjectName);
        }

        public void Dispose()
        {
            IsPipeConnected = false;
            _namedPipesClient.Dispose();
        }
    }
}
