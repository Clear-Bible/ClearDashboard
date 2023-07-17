using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
// https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
#pragma warning disable CA1416

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class CollabProjectManagementViewModel : DashboardApplicationScreen
    {
        private readonly HttpClientServices _httpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private readonly CollaborationConfiguration _collaborationConfiguration;

        #region Member Variables   

        private List<GitUser> _gitLabUsers = new();


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

                if (_selectedProject.Name != "")
                {
                    IsGitLabUserListEnabled = true;
                }
                else
                {
                    IsGitLabUserListEnabled = false;
                }

                _ = GetUsersForProject();
            }
        }

        private ObservableCollection<GitLabProjectUser> _projectUsers = new();
        public ObservableCollection<GitLabProjectUser> ProjectUsers
        {
            get => _projectUsers;
            set
            {
                _projectUsers = value;
                NotifyOfPropertyChange(() => ProjectUsers);
            }
        }

        private ObservableCollection<string> _organization;
        public ObservableCollection<string> Organization
        {
            get => _organization;
            set
            {
                _organization = value;
                NotifyOfPropertyChange(() => Organization);
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

        private bool _isGitLabUserListEnabled;
        public bool IsGitLabUserListEnabled
        {
            get => _isGitLabUserListEnabled;
            set
            {
                _isGitLabUserListEnabled = value;
                NotifyOfPropertyChange(() => IsGitLabUserListEnabled);
            }
        }

        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                CollabeUserCollectionView.Refresh();
                NotifyOfPropertyChange(() => FilterText);

                if (value is null)
                {
                    SelectedOrganization = null;
                    NotifyOfPropertyChange(nameof(SelectedOrganization));
                }
            }
        }


        private string _selectedOrganization;
        public string SelectedOrganization
        {
            get => _selectedOrganization;
            set
            {
                _selectedOrganization = value;
                NotifyOfPropertyChange(() => SelectedOrganization);

                if (value is not null)
                {
                    FilterText = value;
                }
            }
        }

        private ICollectionView _collabeUserCollectionView;
        public ICollectionView CollabeUserCollectionView
        {
            get => _collabeUserCollectionView;
            set
            {
                _collabeUserCollectionView = value;
                NotifyOfPropertyChange(() => CollabeUserCollectionView);
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
            Projects = await _httpClientServices.GetProjectsForUserWhereOwner(ProjectOwner);

            _gitLabUsers = await _httpClientServices.GetAllUsers();
            CollabUsers = new ObservableCollection<GitUser>(_gitLabUsers);

            CollabeUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabeUserCollectionView.Filter = CollabUsersCollectionFilter;

            // get the various orgs
            var org = _gitLabUsers.Select(x => x.Organization).Distinct().ToList();
            Organization = new ObservableCollection<string>(org);


            ShowProgressBar = Visibility.Hidden;

            AttemptToSelectCurrentProject();

            if (SelectedProject.Name is null || SelectedProject.Name == "")
            {
                IsGitLabUserListEnabled = false;
            }

            base.OnViewLoaded(view);
        }



        #endregion //Constructor


        #region Methods

        private bool CollabUsersCollectionFilter(object obj)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                return true;
            }

            if (obj is GitUser user)
            {
                if (user.Name!.ToUpper().Contains(FilterText.ToUpper()) || user.Organization.ToUpper().Contains(FilterText.ToUpper()))
                {
                    return true;
                }
            }
            return false;
        }

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
            NotifyOfPropertyChange(() => CollabUsers);

            CollabeUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabeUserCollectionView.Filter = CollabUsersCollectionFilter;
            CollabeUserCollectionView.Refresh();

            ShowProgressBar = Visibility.Hidden;
        }


        public async void AddUsers(PermissionLevel permissionLevel = PermissionLevel.ReadOnly)
        {
            ShowProgressBar = Visibility.Visible;
            for (int i = CollabUsers.Count - 1; i >= 0; i--)
            {
                var user = CollabUsers[i];
                if (user.IsSelected)
                {
                    _ = await _httpClientServices.AddUserToProject(user, SelectedProject, permissionLevel);
                    CollabUsers.RemoveAt(i);
                    await Task.Delay(500);
                    await GetUsersForProject();

                    CollabeUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
                    CollabeUserCollectionView.Filter = CollabUsersCollectionFilter;
                    CollabeUserCollectionView.Refresh();
                }
            }
            ShowProgressBar = Visibility.Hidden;
        }

        public void AddUsersReadWrite()
        {
            AddUsers(PermissionLevel.ReadWrite);
        }

        public void AddOwner()
        {
            AddUsers(PermissionLevel.Owner);
        }


        public async void RemoveUser()
        {
            ShowProgressBar = Visibility.Visible;
            if (SelectedCurrentUser is not null)
            {
                // you cannot remove yourself
                if (SelectedCurrentUser.UserName != ProjectOwner.RemoteUserName)
                {
                    // you cannot delete the project's true owner
                    if (SelectedCurrentUser.UserName != SelectedProject.RemoteOwner.Username)
                    {
                        await _httpClientServices.RemoveUserFromProject(SelectedCurrentUser, SelectedProject);
                        await Task.Delay(500);
                        await GetUsersForProject();
                    }
                }
            }

            CollabeUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabeUserCollectionView.Filter = CollabUsersCollectionFilter;
            CollabeUserCollectionView.Refresh();

            ShowProgressBar = Visibility.Hidden;
        }

        private async void AttemptToSelectCurrentProject()
        {
            if (ProjectManager?.CurrentProject?.Id == null)
            {
                return;
            }


            foreach (var project in Projects)
            {
                // check to see if this is a Dashboard project with expected naming
                if (project.Name.StartsWith("P_") && project.Name.Length == 38)
                {
                    var guid = project.Name.Substring(2); // removed the leading "P_"

                    if (Guid.Parse(guid) == ProjectManager.CurrentProject.Id)
                    {
                        SelectedProject = project;
                        break;
                    }
                }

            }

            await GetUsersForProject();

        }

        #endregion // Methods

    }
}
