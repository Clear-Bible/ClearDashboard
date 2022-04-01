using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Context;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.Pipes_Shared.Models;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using Pipes_Shared.Models;

namespace ClearDashboard.DataAccessLayer
{
    public class ProjectManager
    {
        #region Properties

        private readonly ILogger _logger;
        private readonly NamedPipesClient _namedPipesClient;
        private readonly ProjectNameDbContextFactory _projectNameDbContextFactory;

        public Project Project;

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
        public string CurrentVerse { get; set; } = "";

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
            else if (pm.Action == ActionType.SetProject)
            {
                // intercept and keep a copy of the current project
                var payload = pm.Payload;
                Project = JsonSerializer.Deserialize<Project>((string)payload);
            } else if (pm.Action == ActionType.CurrentVerse)
            {
                CurrentVerse = pm.Text;
            }
            
            RaisePipesChangedEvent(pm);
        }

        public void GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project's pm directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            var paratextUtils = new Paratext.ParatextUtils();
            var user = paratextUtils.GetCurrentParatextUser();

            ParatextUserName = user;

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
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
    }
}
