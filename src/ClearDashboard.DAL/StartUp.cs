using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer
{


    public class StartUp
    {
        #region props

        private readonly ILogger _logger;
        private readonly NamedPipesClient _namedPipesClient;

        #endregion

        #region Events


        // event handler to be raised when the Paratext Username changes
        public static event EventHandler ParatextUserNameEventHandler;


        public event NamedPipesClient.PipesEventHandler NamedPipeChanged;
        public string ParatextUserName { get; set; } = "";

        private void RaisePipesChangedEvent(PipeMessage pm)
        {
            var args = new PipeEventArgs(pm);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Startup

        public StartUp(NamedPipesClient namedPipeClient, ILogger<StartUp> logger)
        {
            _logger = logger;
            _logger.LogInformation("'DAL.Startup' ctor called.");

            _namedPipesClient = namedPipeClient;

            // Wire up named pipes
            //NamedPipesClient.Instance.InitializeAsync().ContinueWith(t =>
            //        Debug.WriteLine($"Error while connecting to pipe server: {t.Exception}"),
            //    TaskContinuationOptions.OnlyOnFaulted);

            _namedPipesClient.NamedPipeChanged += HandleEvent;
        }

        #endregion

        #region Shutdown

        public void OnClosing()
        {
            _namedPipesClient.Dispose();
        }

        #endregion


        #region Methods

        private void HandleEvent(object sender, PipeEventArgs args)
        {
            RaisePipesChangedEvent(args.PM);
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project'pm directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            Paratext.ParatextUtils paratextUtils = new Paratext.ParatextUtils();
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
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            appPath = Path.Combine(appPath, "ClearDashboard_Projects");

            //check to see if the directory exists already
            string newDir = Path.Combine(appPath, dashboardProject.ProjectName);
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
