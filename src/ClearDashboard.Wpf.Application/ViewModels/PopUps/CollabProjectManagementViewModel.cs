﻿using System;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SIL.Extensions;
using System.Windows;
using MahApps.Metro.Controls;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class CollabProjectManagementViewModel : DashboardApplicationScreen
    {
        private readonly HttpClientServices _httpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private readonly CollaborationConfiguration _collaborationConfiguration;

        #region Member Variables   

        private List<GitUser> _gitLabUsers;

        #endregion //Member Variables

        #region Public Properties

        public CollaborationConfiguration ProjectOwner;

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

                _ = GetUsersForProject();
            }
        }

        private ObservableCollection<GitLabProjectUser> _projectUsers = new();
        public ObservableCollection<GitLabProjectUser>  ProjectUsers
        {
            get => _projectUsers;
            set
            {
                _projectUsers = value;
                NotifyOfPropertyChange(() => ProjectUsers);
            }
        }

        private GitLabProjectUser? _selectedCurrentUser;
        public GitLabProjectUser? SelectedCurrentUser
        {
            get => _selectedCurrentUser;
            set
            {
                _selectedCurrentUser = value;
                NotifyOfPropertyChange(() => SelectedCurrentUser);
            }
        }



        private ObservableCollection<GitUser> _collabUsers = new();
        public ObservableCollection<GitUser> CollabUsers
        {
            get => _collabUsers;
            set
            {
                _collabUsers = value;
                NotifyOfPropertyChange(() => CollabUsers);
            }
        }

        private Visibility _showProgressBar = Visibility.Visible;

        public Visibility ShowProgressBar
        {
            get => _showProgressBar;
            set
            {
                _showProgressBar = value;
                NotifyOfPropertyChange(() => ShowProgressBar);
            }
        }



        #endregion //Observable Properties


        #region Constructor

#pragma warning disable CS8618
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
            _httpClientServices = httpClientServices;
            _collaborationManager = collaborationManager;
            _collaborationConfiguration = collaborationConfiguration;
        }
#pragma warning restore CS8618

        protected override async void OnViewLoaded(object view)
        {
            // get the user's projects
            ProjectOwner = _collaborationManager.GetConfig();
            Projects = await _httpClientServices.GetProjectsForUser(ProjectOwner);

            AttemptToSelectCurrentProject();

            _gitLabUsers = await _httpClientServices.GetAllUsers();
            CollabUsers = new ObservableCollection<GitUser>(_gitLabUsers);

            ShowProgressBar = Visibility.Hidden;

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        public async void Close()
        {
            await TryCloseAsync();
        }

        private async Task GetUsersForProject()
        {
            ShowProgressBar = Visibility.Visible;

            var users = await _httpClientServices.GetUsersForProject(_collaborationConfiguration, SelectedProject.Id);
            ProjectUsers = new ObservableCollection<GitLabProjectUser>(users);

            // remove existing users from the selectable list
            _collabUsers = new ObservableCollection<GitUser>(_gitLabUsers);
            // get the list of current users
            var ids = ProjectUsers.Select(u => u.Id).ToList();
            //
            _collabUsers.RemoveAll(x => ids.Contains(x.Id));
            NotifyOfPropertyChange(() =>  CollabUsers);

            ShowProgressBar = Visibility.Hidden;
        }


        public async void AddUsers()
        {
            ShowProgressBar = Visibility.Visible;
            for (int i = CollabUsers.Count - 1; i >= 0; i--)
            {
                var user = CollabUsers[i];
                if (user.IsSelected)
                {
                    _ = await _httpClientServices.AddUserToProject(user, SelectedProject);
                    CollabUsers.RemoveAt(i);
                    await Task.Delay(500);
                    await GetUsersForProject();
                }
            }
            ShowProgressBar = Visibility.Hidden;
        }

        public async void RemoveUser()
        {
            ShowProgressBar = Visibility.Visible;
            if (SelectedCurrentUser is not null)
            {
                // only remove users who are not the owner
                if (SelectedCurrentUser.IsOwner == false)
                {
                    await _httpClientServices.RemoveUserFromProject(SelectedCurrentUser, SelectedProject);
                    await Task.Delay(500);
                    await GetUsersForProject();
                }
            }
            ShowProgressBar = Visibility.Hidden;
        }

        private void AttemptToSelectCurrentProject()
        {
            if (ProjectManager?.CurrentProject?.Id == null)
            {
                return;
            }


            foreach (var project in Projects)
            {
                var guid = project.Name.Substring(2); // removed the leading "P_"

                if (Guid.Parse(guid) == ProjectManager.CurrentProject.Id)
                {
                    SelectedProject = project;
                    break;
                }
            }

            return;
        }

        #endregion // Methods

    }
}
