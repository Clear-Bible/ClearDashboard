using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class CollabProjectManagementViewModel : DashboardApplicationScreen
    {
        private readonly ILogger<AboutViewModel> _logger;
        private readonly HttpClientServices _httpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private readonly CollaborationConfiguration _collaborationConfiguration;

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties


        private List<GitLabProject> _projects = new();
        public List<GitLabProject> Projects
        {
            get => _projects;
            set
            {
                _projects = value; 
                NotifyOfPropertyChange(() => Projects);
            }
        }


        private GitLabProject _selectedProject = new();
        public GitLabProject SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                NotifyOfPropertyChange(() => SelectedProject);

                GetUsersForProject();
            }
        }

        private List<GitLabProjectUsers> _projectUsers = new();
        public List<GitLabProjectUsers>  ProjectUsers
        {
            get => _projectUsers;
            set
            {
                _projectUsers = value;
                NotifyOfPropertyChange(() => ProjectUsers);
            }
        }



        #endregion //Observable Properties


        #region Constructor

        public CollabProjectManagementViewModel()
        {
            // no-op used by caliburn micro
        }

        public CollabProjectManagementViewModel(INavigationService navigationService,
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService,
            HttpClientServices httpClientServices,
            CollaborationManager collaborationManager,
            CollaborationConfiguration collaborationConfiguration)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _httpClientServices = httpClientServices;
            _collaborationManager = collaborationManager;
            _collaborationConfiguration = collaborationConfiguration;
        }

        protected override async void OnViewLoaded(object view)
        {
            // get the user's projects
            Projects = await _httpClientServices.GetProjectForUser(_collaborationManager.GetConfig());
            var users = await _httpClientServices.GetAllUsers();
            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        public async void Close()
        {
            await TryCloseAsync();
        }

        private async void GetUsersForProject()
        {
            ProjectUsers = await _httpClientServices.GetUsersForProject(_collaborationConfiguration, SelectedProject.Id);
        }

        #endregion // Methods

    }
}
