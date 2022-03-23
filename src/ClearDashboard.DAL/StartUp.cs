using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL.NamedPipes;
using ClearDashboard.DataAccessLayer.Events;
using Microsoft.Extensions.Logging;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer
{


    public class StartUp
    {
        #region props

        private readonly ILogger _logger;

        public bool IsPipeConnected { get; set; }

        public enum PipeAction
        {
            OnConnected,
            OnDisconnected,

            SendText,

            GetBibilicalTerms,
            GetSourceVerses,
            GetTargetVerses,
            GetNotes,
            GetProject,
            GetCurrentVerse,
        }


        #endregion

        #region Events


        // event handler to be raised when the Paratext Username changes
        public static event EventHandler ParatextUserNameEventHandler;


        public event NamedPipesClient.PipesEventHandler NamedPipeChanged;
        public string ParatextUserName { get; set; } = "";

        private void RaisePipesChangedEvent(PipeMessage pm)
        {
            NamedPipesClient.PipeEventArgs args = new NamedPipesClient.PipeEventArgs(pm);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Startup

        public StartUp(ILogger<StartUp> logger)
        {
            _logger = logger;
            _logger.LogInformation("'DAL.Startup' ctor called.");

            // Wire up named pipes
            NamedPipesClient.Instance.InitializeAsync().ContinueWith(t =>
                    Debug.WriteLine($"Error while connecting to pipe server: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);

            NamedPipesClient.Instance.NamedPipeChanged += HandleEvent;
        }

        #endregion

        #region Shutdown

        public void OnClosing()
        {
            IsPipeConnected = false;
            NamedPipesClient.Instance.Dispose();
        }

        #endregion


        #region Methods

        private void HandleEvent(object sender, NamedPipesClient.PipeEventArgs args)
        {
            PipeMessage pm = args.PM;

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

        public async Task SendPipeMessage(PipeAction action, string text = "")
        {
            PipeMessage message = new PipeMessage();
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
                case PipeAction.GetBibilicalTerms:
                    message.Action = ActionType.GetBibilicalTerms;
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
            }

            await NamedPipesClient.Instance.WriteAsync(message);
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
