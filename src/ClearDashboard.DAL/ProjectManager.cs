using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.ViewModels;
using MediatR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer
{

    public abstract class ProjectManager : IDisposable
    {
        #region Properties

        protected ILogger Logger { get; private set; }
        protected ParatextProxy ParatextProxy { get; private set; }
        protected ProjectNameDbContextFactory ProjectNameDbContextFactory { get; private set; }
        protected IMediator Mediator { get; private set; }
        protected HubConnection HubConnection { get; private set; }
        protected IHubProxy HubProxy { get; private set; }
   

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } = new();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } = new();

        public Project ParatextProject { get; protected set; }

        public bool ParatextVisible = false;
        #endregion

        #region Events

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

        #endregion

        #region Startup

        protected ProjectManager(IMediator mediator, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory)
        {
            Logger = logger;
            ProjectNameDbContextFactory = projectNameDbContextFactory;
            ParatextProxy = paratextProxy;
            Logger.LogInformation("'ProjectManager' ctor called.");
            Mediator = mediator;

        }

        protected abstract  Task HookSignalREvents();
        protected abstract Task PublishSignalRConnected(bool connected);
        protected abstract Task PublishParatextUser(string paratextUserName);

        protected async Task ConfigureSignalRClient()
        {
            HubConnection = new HubConnection("http://localhost:9000/signalr");

            HubProxy = HubConnection.CreateHubProxy("Plugin");


            await HookSignalREvents();
            try
            {
                await HubConnection.Start();

                if (HubConnection.State == ConnectionState.Connected)
                {
                    Logger.LogInformation("Connected to SignalR.");
                    HubConnection.Closed += HandleSignalRConnectionClosed;
                    HubConnection.Error += HandleSignalRConnectionError;
                    await PublishSignalRConnected(true);

                }
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError("Paratext is not running, cannot connect to SignalR.");
                await Task.Delay(10);
                await ConfigureSignalRClient();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while trying to connect to Paratext.");
                await Task.Delay(10);
                await ConfigureSignalRClient();
            }
        }

        private async void HandleSignalRConnectionError(Exception obj)
        {
            //var retryTimestamp = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

            //while (DateTime.UtcNow < retryTimestamp)
            //{
            //    await ConfigureSignalRClient();
            //    if (HubConnection.State == ConnectionState.Connected)
            //    {
            //        Logger.LogInformation("SignalR connected.");
            //    }
            //}

            //Logger.LogInformation("SignalR Connection is closed.");
        }

        private async void HandleSignalRConnectionClosed()
        {
            
            var retryTimestamp = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

            while (DateTime.UtcNow < retryTimestamp)
            {
                await ConfigureSignalRClient();
                if (HubConnection.State == ConnectionState.Connected)
                {
                    Logger.LogInformation("SignalR connected.");
                    return;
                }
            }

            Logger.LogInformation("SignalR Connection is closed.");
        }

        #endregion

       

        #region Methods


        public async Task Initialize()
        {
            await GetParatextUserName();
            EnsureDashboardProjectDirectory();
            await ConfigureSignalRClient();
            
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
                    Logger.LogError(ex,
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

            ParatextVisible = ParatextProxy.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                var projects = await ParatextProxy.GetParatextProjectsOrResources(ParatextProxy.FolderType.Projects);
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
                    Logger.LogError(ex, "Unexpected error while initializing");
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                var resources = ParatextProxy.GetParatextResources();
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
                    Logger.LogError(ex, "Unexpected error while initializing");
                }
            }
        }

        public async Task GetParatextUserName()
        {
            // TODO this is a hack that reads the first user in the Paratext project's pm directory
            // from the localUsers.txt file.  This needs to be changed to the user we get from 
            // the Paratext API
            var user = ParatextProxy.GetCurrentParatextUser();

            ParatextUserName = user;

            await PublishParatextUser(ParatextUserName);
          
        }

        public DashboardProject CurrentDashboardProject { get; set; }


        public DashboardProject CreateDashboardProject()
        {
            CurrentDashboardProject = new DashboardProject
            {
                ParatextUser = ParatextUserName,
                CreationDate = DateTime.Now
            };

            return CurrentDashboardProject;
        }


        public async Task CreateNewProject(DashboardProject dashboardProject)
        {
            var projectAssets = await ProjectNameDbContextFactory.Get(dashboardProject.ProjectName);
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
           
        }


        #endregion

        #region Commands

        public Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return Mediator.Send(request, cancellationToken);
        }
        #endregion

    }
}
