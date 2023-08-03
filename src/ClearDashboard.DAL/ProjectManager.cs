using Autofac;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Exceptions;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Features.Projects;
using ClearDashboard.DataAccessLayer.Features.User;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer
{
    public abstract class ProjectManager : IUserProvider, IProjectProvider, IProjectManager, IDisposable
    {
#nullable disable
        #region Properties

        protected ILogger Logger { get; }
        protected ParatextProxy ParatextProxy { get; }
        protected ILifetimeScope LifetimeScope { get; }

        #region IUserProvider
        public User CurrentUser { get; set; }
        //public string ParatextUserName { get; set; } = "";

        #endregion IUserProvider

        #region IProjectProvider
        public Project CurrentProject { get; set; }
        public ParatextProject CurrentParatextProject { get; set; }
        public bool HasCurrentProject => CurrentProject != null;
        public bool CanRunDenormalization => (CurrentProject != null && !PauseDenormalization);
        public bool PauseDenormalization { get; set; }
        public bool HasCurrentParatextProject => CurrentParatextProject != null;

        private string _currentVerse;
        public string CurrentVerse
        {
            get => _currentVerse;
            set
            {
                // ensure that we are getting a fully delimited BBB as things like
                // 01 through 09 often get truncated to "1" through "9" without the 
                // leading zero
                var s = value;
                if (s.Length < "BBBCCCVVV".Length)
                {
                    s = value.PadLeft("BBBCCCVVV".Length, '0');
                }
                _currentVerse = s;
            }
        }

        #endregion IProjectProvider


       


      

   
        #endregion

        #region Events

        #endregion

        #region Startup

        protected ProjectManager(ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ILifetimeScope lifetimeScope)
        {
            Logger = logger;
            LifetimeScope = lifetimeScope;
            ParatextProxy = paratextProxy;
            Logger.LogInformation("'ProjectManager' ctor called.");
        }


        #endregion

        #region Methods

        protected abstract Task PublishParatextUser(User user);

        public virtual async Task Initialize()
        {
            EnsureDashboardDirectory(FilePathTemplates.ProjectBaseDirectory);
            await Task.CompletedTask;
        }

        private void EnsureDashboardDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!Directory.Exists(directory))
            {
                // need to create that directory
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        $"An unexpected error occurred while creating '{directory}");
                }
            }
        }


        // TODO:  Review - should we be throwing an exception here if a licensed user can not be loaded?
        private Guid TemporaryUserGuid => Guid.Parse("5649B1D2-2766-4C10-9274-F7E7BF75E2B7");
        public bool SetCurrentUserFromLicense()
        {
            var user = LicenseManager.GetUserFromLicense();

            if (user.Id == Guid.Empty)
            {
                if (CurrentUser == null)
                {
                    user.Id = TemporaryUserGuid;
                    CurrentUser = user;
                    CheckForCurrentUser();
                }
                return false;
            }

            if (!File.Exists(LicenseManager.LicenseFilePath))
            {
                LicenseManager.EncryptToFile(user, LicenseManager.LicenseFolderPath);
            }

            CurrentUser = user;
            CheckForCurrentUser();

            return true;
        }

        public void CheckForCurrentUser()
        {
            if (CurrentProject != null && CurrentProject.ProjectName != null)
            {
                var requestResults = ExecuteRequest(new GetProjectUserSlice.GetProjectUserQuery(CurrentProject.ProjectName), CancellationToken.None);

                if (requestResults.IsCompleted && requestResults.Result.Success && requestResults.Result.HasData)
                {
                    var currentUserIsInDatabase = false;

                    foreach (var item in requestResults.Result.Data)
                    {
                        if (item.Id == CurrentUser.Id)
                        {
                            currentUserIsInDatabase = true;
                        }
                    }

                    if (!currentUserIsInDatabase)
                    {
                        ExecuteRequest(new AddUserCommand(CurrentUser), CancellationToken.None);
                    }
                }
            }
        }

        public async Task<User> UpdateCurrentUserWithParatextUserInformation()
        {
          
            if (CurrentUser == null || CurrentUser.Id == TemporaryUserGuid)
            {
                SetCurrentUserFromLicense();
            }

            var result = await ExecuteRequest(new GetCurrentParatextUserQuery(), CancellationToken.None);

            Logger.LogInformation(result.Success ? $"Found Paratext user - {result.Data!.Name}" : $"GetParatextUserName - {result.Message}");


            if (result.Success && result.HasData)
            {
                var paratextUserName = result.Data!.Name;
                SetCurrentUserFromLicense();
                CurrentUser.ParatextUserName = paratextUserName;
                await PublishParatextUser(CurrentUser);
            }
            else
            {
                CurrentUser.FirstName = string.Empty;
                CurrentUser.LastName = string.Empty;
            }

            return CurrentUser;
        }

        public DashboardProject CurrentDashboardProject { get; set; }

        public bool HasDashboardProject => CurrentDashboardProject != null;


        public DashboardProject CreateDashboardProject()
        {
            CurrentDashboardProject = new DashboardProject
            {
               // ParatextUser = ParatextUserName,
                Created = DateTime.Now
            };

            return CurrentDashboardProject;
        }

        public async Task CreateNewProject(string projectName)
        {
            CreateDashboardProject();

            var projectSanitizedName = ProjectDbContextFactory.ConvertProjectNameToSanitizedName(projectName);

            // Seed the IProjectProvider implementation for ProjectDbContext to use
            // when creating the database:
            CurrentProject = new Project
            {
                ProjectName = projectSanitizedName
            };

            // Create the project directory:
            CurrentDashboardProject.DirectoryPath = string.Format(FilePathTemplates.ProjectDirectoryTemplate, projectSanitizedName);
            EnsureDashboardDirectory(CurrentDashboardProject.DirectoryPath);

            CurrentProject = await CreateProject(projectName);
            CurrentDashboardProject.ProjectName = projectSanitizedName;
        }

        public void Dispose()
        {
           //no-op for now  
        }


        #endregion

        #region Commands

        public Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var mediator = LifetimeScope.Resolve<IMediator>();
            return mediator.Send(request, cancellationToken);
        }
        #endregion

        public async Task<Project> LoadProject(string projectName)
        {
            var result = await ExecuteRequest(new LoadProjectQuery(projectName), CancellationToken.None);

            if (result.Success && result.HasData)
            {
                CurrentProject = result.Data;

                if (CurrentDashboardProject != null)
                {
                    CurrentDashboardProject.DirectoryPath = string.Format(
                        FilePathTemplates.ProjectDirectoryTemplate,
                        ProjectDbContextFactory.ConvertProjectNameToSanitizedName(CurrentProject!.ProjectName!));
                }

                return CurrentProject;
            }

            throw new CouldNotLoadProjectException(projectName);
        }

        public async Task<Project> DeleteProject(string projectName)
        {
            var result = await ExecuteRequest(new DeleteProjectCommand(projectName), CancellationToken.None);
            return result.Data;
        }

        public async Task<Project> CreateProject(string projectName)
        {
            var result = await ExecuteRequest(new CreateProjectCommand(projectName, CurrentUser ), CancellationToken.None);
            if (result.Success)
            {
                return result.Data;
            }
            else
            {
                var message = $"Could not create a project: {result.Message}";
                Logger.LogError(message);
                throw new ApplicationException(message);
            } 
        }

        public async Task<Project> UpdateProject(Project project)
        {
            var result = await ExecuteRequest(new UpdateProjectCommand(project), CancellationToken.None);
            return result.Data;
        }
    }
}
