using System;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer
{
    public class ProjectManager
    {
        #region props

        private readonly ILogger _logger;
        private readonly NamedPipesClient _namedPipesClient;

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

        #region Startup

        public ProjectManager(NamedPipesClient namedPipeClient, ILogger<ProjectManager> logger)
        {
            _logger = logger;
            _logger.LogInformation("'ProjectManager' ctor called.");

            _namedPipesClient = namedPipeClient;
            _namedPipesClient.NamedPipeChanged += HandleNamedPipeChanged;
        }

        #endregion

        #region Shutdown

        public void OnClosing()
        {
            _namedPipesClient.Dispose();
        }

        #endregion


        #region Methods

        private void HandleNamedPipeChanged(object sender, PipeEventArgs args)
        {
            RaisePipesChangedEvent(args.PM);
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project'pm directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            var paratextUtils = new Paratext.ParatextUtils();
            var user = paratextUtils.GetCurrentParatextUser();

            ParatextUserName = user;

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
        }

        public async Task SendMessage(string message)
        {
            message = message.Trim() + " through the DAL";

            await _namedPipesClient.WriteAsync(message);
        }

        #endregion


        public async Task CreateNewProject(DashboardProject dashboardProject)
        {
            // create the new folder
            var appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            appPath = Path.Combine(appPath, "ClearDashboard_Projects");

            //check to see if the directory exists already
            var newDir = Path.Combine(appPath, dashboardProject.ProjectName);
            if (!Directory.Exists(newDir))
            {
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"'DAL.CreateNewProject: {ex.Message}");
                    return;
                }
            }

            dashboardProject.ProjectPath = newDir;

            // PASS THIS DOWN TO MICHAEL FOR DATABASE CREATION


            // remove compiler warning
            await Task.CompletedTask;

        }
    }
}
