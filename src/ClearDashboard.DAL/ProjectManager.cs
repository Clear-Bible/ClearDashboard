using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.ViewModels;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using MediatR;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ClearDashboard.DataAccessLayer.Data.Models;
using ClearDashboard.DataAccessLayer.Features.Projects;

namespace ClearDashboard.DataAccessLayer
{


    public abstract class ProjectManager : IUserProvider, IProjectProvider, IProjectManager, IDisposable
    {
#nullable disable
        #region Properties
        public Guid ManuscriptHebrewGuid = Guid.Parse("5db213425b714efc9dd23794525058a4");
        public Guid ManuscriptGreekGuid = Guid.Parse("5db213425b714efc9dd23794525058a5");

        protected ILogger Logger { get; private set; }
        protected ParatextProxy ParatextProxy { get; private set; }
        protected ILifetimeScope LifetimeScope { get; private set; }
        public User CurrentUser { get; set; }
        public Project CurrentProject { get; set; }
        public ParatextProject CurrentParatextProject { get; set; }
        public bool HasCurrentProject => CurrentProject != null;
        public bool HasCurrentParatextProject => CurrentParatextProject != null;
        
        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } = new();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } = new();
        public bool ParatextVisible = false;
        public string ParatextUserName { get; set; } = "";
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

        private Dictionary<string, string> _bcvDictionary = new();
        private Dictionary<string, string> BCVDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
            }
        }

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
        }

        private Guid TemporaryUserGuid => Guid.Parse("5649B1D2-2766-4C10-9274-F7E7BF75E2B7");
        public User GetLicensedUser()
        {
            var user = LicenseManager.GetUser<User>();
            if (user == null)
            {
                user = new User
                {
                    Id = TemporaryUserGuid,
                };
            }

            return user;
        }

        public async Task<User> UpdateCurrentUserWithParatextUserInformation()
        {
          
            if (CurrentUser == null || CurrentUser.Id == TemporaryUserGuid)
            {
                CurrentUser = GetLicensedUser();
            }

            var result = await ExecuteRequest(new GetCurrentParatextUserQuery(), CancellationToken.None);

            Logger.LogInformation(result.Success ? $"Found Paratext user - {result.Data.Name}" : $"GetParatextUserName - {result.Message}");


            if (result.Success && result.HasData)
            {
                var paratextUserName = result.Data.Name;
                CurrentUser = GetLicensedUser();
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
                ParatextUser = ParatextUserName,
                Created = DateTime.Now
            };

            return CurrentDashboardProject;
        }

        public async Task CreateNewProject(string projectName)
        {
            CreateDashboardProject();

            // Seed the IProjectProvider implementation.
            CurrentProject = new Project
            {
                ProjectName = projectName
            };
            CurrentProject = await CreateProject(projectName);
            CurrentDashboardProject.ProjectName = CurrentProject.ProjectName;
        }

        public void Dispose()
        {
            LifetimeScope.Dispose();
        }


        #endregion

        #region Commands

        public Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var mediator = LifetimeScope.Resolve<IMediator>();
            return mediator.Send(request, cancellationToken);
        }
        #endregion
        
        public Task<IEnumerable<Project>> GetAllProjects()
        {
            throw new NotImplementedException();
        }

        public async Task<Project> LoadProject(string projectName)
        {
            using var requestScope = LifetimeScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            var projectDbContextFactory = LifetimeScope.Resolve<ProjectDbContextFactory>();
            var projectDbContext = await projectDbContextFactory.GetDatabaseContext(
                projectName,
                false,
                requestScope);

            CurrentProject = projectDbContext.Projects.First();

            return CurrentProject;
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

        public async Task UpdateProject(Project project)
        {
            using var requestScope = LifetimeScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            var projectDbContextFactory = LifetimeScope.Resolve<ProjectDbContextFactory>();
            var projectDbContext = await projectDbContextFactory.GetDatabaseContext(
                project.ProjectName, 
                false,
                requestScope);

            Logger.LogInformation($"Saving the design surface layout for project '{CurrentProject.ProjectName}'");
            projectDbContext.Update(project);

            await projectDbContext.SaveChangesAsync();

            Logger.LogInformation($"Saved the design surface layout for project '{CurrentProject.ProjectName}'");
        }
    }
}
