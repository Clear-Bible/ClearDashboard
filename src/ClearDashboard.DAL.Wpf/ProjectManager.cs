using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Context;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.ViewModels;
using MediatR;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using Pipes_Shared;
using Pipes_Shared.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.DataAccessLayer.Wpf
{
    public class ProjectManager : IDisposable
    {
        #region Properties

        private readonly ILogger _logger;
        private readonly NamedPipesClient _namedPipesClient;
        private readonly ParatextProxy _paratextProxy;
        private readonly ProjectNameDbContextFactory _projectNameDbContextFactory;
        private readonly IMediator _mediator;

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } = new ();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } = new ();

        public Project ParatextProject { get; private set; }

        public bool ParatextVisible = false;

        public bool IsPipeConnected { get; set; }

      

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
            GetUSX,
        }
        #endregion

        #region Events

        // event handler to be raised when the Paratext Username changes
        public event EventHandler ParatextUserNameEventHandler;
        public event NamedPipesClient.PipesEventHandler NamedPipeChanged;
        public string ParatextUserName { get; set; } = "";


        private string _currentVerse;
        public string CurrentVerse
        {
            get { return _currentVerse; }
            set
            {
                // ensure that we are getting a fully delimited BB as things like
                // 01 through 09 often get truncated to "1" through "9" without the 
                // leading zero
                var s = value;
                if (s.Length < "BBCCCVVV".Length)
                {
                    s = value.PadLeft("BBCCCVVV".Length, '0');
                }
                _currentVerse = s;
            }
        }


        private void RaisePipesChangedEvent(PipeMessage pm)
        {
            var args = new PipeEventArgs(pm);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Startup

        public ProjectManager(IMediator mediator, NamedPipesClient namedPipeClient, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory)
        {
            _logger = logger;
            _projectNameDbContextFactory = projectNameDbContextFactory;
            _paratextProxy = paratextProxy;
            _logger.LogInformation("'ProjectManager' ctor called.");

            _namedPipesClient = namedPipeClient;
            _namedPipesClient.NamedPipeChanged += HandleNamedPipeChanged;

            _mediator = mediator;
        }

        #endregion
          
        #region Shutdown

        public void OnClosing()
        {
            IsPipeConnected = false;
            _namedPipesClient.NamedPipeChanged -= HandleNamedPipeChanged;
            _namedPipesClient.DisposeAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Methods

        private void HandleNamedPipeChanged(object sender, PipeEventArgs args)
        {
            var pipeMessage = args.PipeMessage;

            switch (pipeMessage.Action)
            {
                case ActionType.OnConnected:
                    IsPipeConnected = true;
                    break;
                case ActionType.OnDisconnected:
                    IsPipeConnected= false;
                    break;
                case ActionType.SetProject:
                {
                    // intercept and keep a copy of the current project
                    var payload = pipeMessage.Payload;
                    ParatextProject = JsonSerializer.Deserialize<Project>((string)payload);
                    break;
                }
                case ActionType.CurrentVerse:
                    CurrentVerse = pipeMessage.Text;
                    break;
                default:
                    // Nothing to do.
                    break;
            }
            
            RaisePipesChangedEvent(pipeMessage);
        }

        public void Initialize()
        {
            GetParatextUserName();
            EnsureDashboardProjectDirectory();
        }

        private void EnsureDashboardProjectDirectory()
        {
            if (!Directory.Exists(FilePathTemplates.ProjectBaseDirectory))
            {
                // need to create that directory
                try
                {
                    Directory.CreateDirectory(FilePathTemplates.ProjectBaseDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"An unexpected error occurred while creating '{FilePathTemplates.ProjectBaseDirectory}");
                }
            }
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
           
            ParatextVisible = _paratextProxy.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                var projects = await _paratextProxy.GetParatextProjectsOrResources(ParatextProxy.FolderType.Projects);
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
                var resources = _paratextProxy.GetParatextResources();
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
            var user = _paratextProxy.GetCurrentParatextUser();

            ParatextUserName = user;

            // raise the paratext username event
            ParatextUserNameEventHandler?.Invoke(this, new CustomEvents.ParatextUsernameEventArgs(user));
        }

        public DashboardProject CurrentDashboardProject { get; set; }
        public FlowDirection CurrentLanguageFlowDirection { get; set; }

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
                case PipeAction.GetUSX:
                    message.Action = ActionType.GetUSX;
                    message.Text = text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            await _namedPipesClient.WriteAsync(message);
        }

        public async Task CreateNewProject(DashboardProject dashboardProject)
        {
            var projectAssets = await _projectNameDbContextFactory.Create(dashboardProject.ProjectName);
            // Populate ProjectInfo table
            // Identify relationships
            //   1. Create ParallelCorpus per green line, which includes Corpus, getting back ParallelCorpusId and CorpaIds
            //   2.Manuscript to target (use ToDb.ManuscriptParatextParallelCorporaToDb)
            //      a. Get manuscript from Clear.Engine (SourceCorpus)
            //      b. Get Target from Paratext (TargetCorpus)
            //      c. Parallelize 
            //
            //      d.  Insert results into 

            //dashboardProject.TargetProject.
        }

        public void Dispose()
        {
            IsPipeConnected = false;
            _namedPipesClient.NamedPipeChanged -= HandleNamedPipeChanged;
            _namedPipesClient.DisposeAsync().GetAwaiter().GetResult();
        }


        #endregion

        #region Commands

        public Task<TResponse> ExecuteCommand<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return _mediator.Send(request, cancellationToken);
        }
        #endregion

    }
}
