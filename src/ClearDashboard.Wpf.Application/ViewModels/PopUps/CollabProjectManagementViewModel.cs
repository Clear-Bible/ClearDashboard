using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Popups;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

// https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416
#pragma warning disable CA1416

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class CollabProjectManagementViewModel : DashboardApplicationScreen
    {
        private readonly ILogger<AboutViewModel> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly GitLabHttpClientServices _gitLabHttpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private readonly CollaborationConfiguration _collaborationConfiguration;
        private readonly IWindowManager _windowManager;

        #region Member Variables   

        private List<GitUser> _gitLabUsers = new();
        private bool _inStartup = true;
        private List<GitLabProjectUser> _totalProjectUsers;


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

                if (_inStartup || _selectedProject is null)
                {
                    IsGitLabUserListEnabled = false;
                    return;
                }

                if (_selectedProject.Name != "")
                {
                    IsGitLabUserListEnabled = true;
                }

                _ = GetUsersForProject();


                if (ProjectOwner.RemoteUserName == SelectedProject.RemoteOwner.Username)
                {
                    ShowDeleteProjectPanel = Visibility.Visible;
                }
                else
                {
                    ShowDeleteProjectPanel = Visibility.Hidden;
                }

            }
        }


        private Visibility _showDeleteProjectPanel = Visibility.Hidden;
        public Visibility ShowDeleteProjectPanel
        {
            get => _showDeleteProjectPanel;
            set
            {
                _showDeleteProjectPanel = value;
                NotifyOfPropertyChange(() => ShowDeleteProjectPanel);
            }
        }



        private List<GitLabProjectUser> _projectUsers = new();
        public List<GitLabProjectUser> ProjectUsers
        {
            get => _projectUsers;
            set
            {

                _projectUsers = value;

                ProjectParticipants.Clear();
                ProjectOwners.Clear();
                foreach (var user in _projectUsers)
                {
                    if (user.IsOwner)
                    {
                        ProjectOwners.Add(user);
                    }
                    else
                    {
                        ProjectParticipants.Add(user);
                    }
                }

                NotifyOfPropertyChange(() => ProjectUsers);
            }
        }

        private ObservableCollection<GitLabProjectUser> _projectParticipants = new();
        public ObservableCollection<GitLabProjectUser> ProjectParticipants
        {
            get => _projectParticipants;
            set
            {
                _projectParticipants = value;
                NotifyOfPropertyChange(() => ProjectParticipants);
            }
        }

        private ObservableCollection<GitLabProjectUser> _projectOwners = new();
        public ObservableCollection<GitLabProjectUser> ProjectOwners
        {
            get => _projectOwners;
            set
            {
                _projectOwners = value;
                NotifyOfPropertyChange(() => ProjectOwners);
            }
        }

        // public ObservableCollection<GitLabProjectUser> ProjectParticipants => ProjectUsers .(x => x.IsOwner);

        //public ObservableCollection<GitLabProjectUser> ProjectOwners => (ObservableCollection<GitLabProjectUser>)ProjectUsers.Where(x => !x.IsOwner);

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
                CollabUserCollectionView.Refresh();
                NotifyOfPropertyChange(() => FilterText);
            }
        }

        private string _selectedOrganization;
        public string SelectedOrganization
        {
            get => _selectedOrganization;
            set
            {
                _selectedOrganization = value;
                CollabUserCollectionView.Refresh();
                NotifyOfPropertyChange(() => SelectedOrganization);
            }
        }

        private ICollectionView _collabUserCollectionView;
        public ICollectionView CollabUserCollectionView
        {
            get => _collabUserCollectionView;
            set
            {
                _collabUserCollectionView = value;
                NotifyOfPropertyChange(() => CollabUserCollectionView);
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
            GitLabHttpClientServices gitLabHttpClientServices,
            CollaborationManager collaborationManager,
            CollaborationConfiguration collaborationConfiguration,
            IWindowManager windowManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _localizationService = localizationService;
            _gitLabHttpClientServices = gitLabHttpClientServices;
            _collaborationManager = collaborationManager;
            _collaborationConfiguration = collaborationConfiguration;
            _windowManager = windowManager;
        }
#pragma warning restore CS8618

        protected override async void OnViewLoaded(object view)
        {
            // get the user's projects
            ProjectOwner = _collaborationManager.GetConfig();


            Projects = await _gitLabHttpClientServices.GetProjectsForUserWhereOwner(ProjectOwner);
            _gitLabUsers = await _gitLabHttpClientServices.GetAllUsers();
            CollabUsers = new ObservableCollection<GitUser>(_gitLabUsers);

            CollabUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabUserCollectionView.Filter = CollabUsersCollectionFilter;

            // get the various orgs
            var org = _gitLabUsers.Select(x => x.Organization).Distinct().ToList();
            Organization = new ObservableCollection<string>(org);


            _inStartup = false;
            AttemptToSelectCurrentProject();

            if (SelectedProject is null)
            {
                IsGitLabUserListEnabled = false;
            }
            else
            {
                if (SelectedProject.Name == string.Empty)
                {
                    IsGitLabUserListEnabled = false;
                }
            }


            ShowProgressBar = Visibility.Hidden;

            base.OnViewLoaded(view);
        }



        #endregion //Constructor


        #region Methods

        private bool CollabUsersCollectionFilter(object obj)
        {
            if (obj is GitUser user)
            {
                if (user.Name!.ToUpper().Contains((FilterText ?? string.Empty).ToUpper()) &&
                    user.Organization.ToUpper().Contains((SelectedOrganization ?? string.Empty).ToUpper()))
                {
                    return true;
                }
            }
            return false;
        }

        public async void Close()
        {
            await EventAggregator.PublishOnUIThreadAsync(new ReloadProjectPickerProjects());

            await TryCloseAsync();
        }

        private async Task GetUsersForProject()
        {
            if (SelectedProject is null)
            {
                return;
            }


            ShowProgressBar = Visibility.Visible;

            
            _totalProjectUsers = await _gitLabHttpClientServices.GetUsersForProject(_collaborationConfiguration, SelectedProject.Id);
            ProjectUsers = new List<GitLabProjectUser>(_totalProjectUsers);

            // remove existing users from the selectable list
            _collabUsers = new ObservableCollection<GitUser>(_gitLabUsers);
            // get the list of current users
            var ids = ProjectUsers.Select(u => u.Id).ToList();
            //
            _collabUsers.RemoveAll(x => ids.Contains(x.Id));
            NotifyOfPropertyChange(() => CollabUsers);

            CollabUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabUserCollectionView.Filter = CollabUsersCollectionFilter;
            CollabUserCollectionView.Refresh();

            ShowProgressBar = Visibility.Hidden;
        }


        public async void AddUsers(PermissionLevel permissionLevel)
        {
            ShowProgressBar = Visibility.Visible;
            for (int i = CollabUsers.Count - 1; i >= 0; i--)
            {
                var user = CollabUsers[i];
                if (user.IsSelected)
                {
                    _ = await _gitLabHttpClientServices.AddUserToProject(user, SelectedProject, permissionLevel);
                    CollabUsers.RemoveAt(i);
                    await Task.Delay(500);
                    await GetUsersForProject();

                    CollabUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
                    CollabUserCollectionView.Filter = CollabUsersCollectionFilter;
                    CollabUserCollectionView.Refresh();
                    user.IsSelected = false;
                }
            }
            ShowProgressBar = Visibility.Hidden;
        }

        public void AddUsersReadWrite()
        {
            AddUsers(PermissionLevel.ReadWrite);
        }

        public void AddUsersReadOnly()
        {
            AddUsers(PermissionLevel.ReadOnly);
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
                        await _gitLabHttpClientServices.RemoveUserFromProject(SelectedCurrentUser, SelectedProject);
                        await Task.Delay(500);
                        await GetUsersForProject();
                    }
                }
            }

            CollabUserCollectionView = CollectionViewSource.GetDefaultView(CollabUsers);
            CollabUserCollectionView.Filter = CollabUsersCollectionFilter;
            CollabUserCollectionView.Refresh();

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

        public async void SetCheckBox(object sender)
        {
            if (sender is GitUser user)
            {
                user.IsSelected = !user.IsSelected;
            }
            CollabUserCollectionView.Refresh();
        }


        public async void DeleteProject()
        {
            if (SelectedProject is null)
            {
                return;
            }

            var users = _totalProjectUsers;

            users.RemoveAll(x => x.Id == ProjectOwner.UserId);

            // confirm the user wants to delete the project
            var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

            if (confirmationViewPopupViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
            }

            confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.DeleteProjectConfirmation;

            if (users.Count > 0)
            {
                // show extended confirmation dialog as there are other users
                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.DeleteCollabProjectExtended;
            }
            else
            {
                // show simple confirmation dialog
                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.DeleteCollabProjectSimple;
            }

            bool result = false;
            OnUIThread(async () =>
            {
                result = await _windowManager!.ShowDialogAsync(confirmationViewPopupViewModel, null,
                    SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));
            });

            if (result)
            {
                var ret = await _gitLabHttpClientServices.DeleteProject(SelectedProject);
                if (ret)
                {
                    await Task.Delay(500);
                    Projects.Remove(SelectedProject);
                }

                // remove the collaboration project directory
                try
                {
                    var path = Path.Combine(_collaborationManager.GetRespositoryBasePath(), SelectedProject.Path);
                    if (Directory.Exists(path))
                    {
                        FileAttributesHelper.SetNormalFileAttributes(path);

                        var directoryInfo = new DirectoryInfo(path);
                        foreach (var file in directoryInfo.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (var directory in directoryInfo.GetDirectories())
                        {
                            directory.Delete(true);
                        }

                        directoryInfo.Delete();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error deleting collaboration project directory");
                }

                // clean up the UI
                _collabUsers = new ObservableCollection<GitUser>(_gitLabUsers);
                ProjectParticipants = new ObservableCollection<GitLabProjectUser>();
                ProjectOwners = new ObservableCollection<GitLabProjectUser>();
                SelectedCurrentUser = null;


                // update the project's SHA in the database
                Guid guid = Guid.Parse(SelectedProject.Name.Substring(2)); // remove the leading "P_"
                await EventAggregator.PublishOnUIThreadAsync(new DeletedGitProject(guid));

                SelectedProject = null;
                ShowDeleteProjectPanel = Visibility.Hidden;
            }
        }

        #endregion // Methods

    }
}
