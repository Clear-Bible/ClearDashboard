using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GenerateLicenseKeyForDashboard.Models;
using LicenseManager = ClearDashboard.DataAccessLayer.LicenseManager;
using System.Text.RegularExpressions;


namespace GenerateLicenseKeyForDashboard.ViewModels
{
    public class ShellViewModel : Screen
    {
        // ReSharper disable global MemberCanBePrivate.Global

        #region Member Variables   

        private readonly int _licenseVersion = 2;
        private readonly CollaborationServerHttpClientServices _mySqlHttpClientServices;
        private readonly GitLabHttpClientServices _gitLabServices;


        /// <summary>
        /// Function to generate a user name from first & lastnames
        /// </summary>
        /// <returns></returns>
        private string GetUserName() => (Regex.Replace(FirstNameBox, @"\s|\p{P}|[']", "") + "." + Regex.Replace(LastNameBox, @"\s|\p{P}|[']", "")).ToLower();


        private CollaborationConfiguration _collaborationConfiguration;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties


        private bool _isCreateButtonEnabled = false;
        public bool IsCreateButtonEnabled
        {
            get => _isCreateButtonEnabled;
            set => Set(ref _isCreateButtonEnabled, value);
        }



        private Visibility _fetchByEmailInput;
        public Visibility FetchByEmailInput
        {
            get => _fetchByEmailInput;
            set => Set(ref _fetchByEmailInput, value);
        }


        private Visibility _fetchByIdInput;
        public Visibility FetchByIdInput
        {
            get => _fetchByIdInput;
            set => Set(ref _fetchByIdInput, value);
        }


        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }


        private bool _emailChecked;
        public bool EmailChecked
        {
            get => _emailChecked;
            set => Set(ref _emailChecked, value);
        }

        private string _emailDelete;
        public string EmailDelete
        {
            get => _emailDelete;
            set => Set(ref _emailDelete, value);
        }



        private bool _idChecked;
        public bool IdChecked
        {
            get => _idChecked;
            set => Set(ref _idChecked, value);
        }


        private bool _isInternalChecked;
        public bool IsInternalChecked
        {
            get => _isInternalChecked;
            set => Set(ref _isInternalChecked, value);
        }

        private string _emailBox = string.Empty;
        public string EmailBox
        {
            get => _emailBox;
            set
            {
                value = value.ToLower();

                Set(ref _emailBox, value);
                ValidateCreateButton();
            }
        }


        private string _generatedLicenseBoxText = string.Empty;
        public string GeneratedLicenseBoxText
        {
            get => _generatedLicenseBoxText;
            set => Set(ref _generatedLicenseBoxText, value);
        }



        private string _firstNameBox = string.Empty;
        public string FirstNameBox
        {
            get => _firstNameBox;
            set
            {
                // remove any non-alphanumeric characters
                Regex rgx = new Regex("[^_a-zA-Z\\d\\s -]");
                value = rgx.Replace(value, "");

                // replace spaces with underscore otherwise this won't work as a username
                value = value.Replace(' ', '_');

                Set(ref _firstNameBox, value);
                ValidateCreateButton();
            }
        }

        private string _lastNameBox = string.Empty;
        public string LastNameBox
        {
            get => _lastNameBox;
            set
            {
                // remove any non-alphanumeric characters
                Regex rgx = new Regex("[^_a-zA-Z\\d\\s -]");
                value = rgx.Replace(value, "");

                // replace spaces with underscore otherwise this won't work as a username
                value = value.Replace(' ', '_');


                Set(ref _lastNameBox, value);
                ValidateCreateButton();
            }
        }


        private string _licenseDecryptionBox = string.Empty;
        public string LicenseDecryptionBox
        {
            get => _licenseDecryptionBox;
            set => Set(ref _licenseDecryptionBox, value);
        }


        private string _decryptedFirstNameBox = string.Empty;
        public string DecryptedFirstNameBox
        {
            get => _decryptedFirstNameBox;
            set => Set(ref _decryptedFirstNameBox, value);
        }

        private string _decryptedLastNameBox = string.Empty;
        public string DecryptedLastNameBox
        {
            get => _decryptedLastNameBox;
            set => Set(ref _decryptedLastNameBox, value);
        }

        private string _decryptedGuidBox = string.Empty;
        public string DecryptedGuidBox
        {
            get => _decryptedGuidBox;
            set => Set(ref _decryptedGuidBox, value);
        }

        private string _decryptedLicenseVersionBox = string.Empty;
        public string DecryptedLicenseVersionBox
        {
            get => _decryptedLicenseVersionBox;
            set => Set(ref _decryptedLicenseVersionBox, value);
        }


        private bool _decryptedInternalCheckBox;
        public bool DecryptedInternalCheckBox
        {
            get => _decryptedInternalCheckBox;
            set => Set(ref _decryptedInternalCheckBox, value);
        }


        private string _fetchedEmailBox = string.Empty;
        public string FetchedEmailBox
        {
            get => _fetchedEmailBox;
            set => Set(ref _fetchedEmailBox, value);
        }


        private string _fetchedLicenseBox = string.Empty;
        public string FetchedLicenseBox
        {
            get => _fetchedLicenseBox;
            set => Set(ref _fetchedLicenseBox, value);
        }


        private string _fetchByIdBox = string.Empty;
        public string FetchByIdBox
        {
            get => _fetchByIdBox;
            set => Set(ref _fetchByIdBox, value);
        }


        private string _fetchByEmailBox = string.Empty;
        public string FetchByEmailBox
        {
            get => _fetchByEmailBox;
            set => Set(ref _fetchByEmailBox, value);
        }


        private string _combinedLicense = string.Empty;
        public string CombinedLicense
        {
            get => _combinedLicense;
            set => Set(ref _combinedLicense, value);
        }


        private string _combinedCreateLicense = string.Empty;
        public string CombinedCreateLicense
        {
            get => _combinedCreateLicense;
            set => Set(ref _combinedCreateLicense, value);
        }


        private string _deleteByIdBox = string.Empty;
        public string DeleteByIdBox
        {
            get => _deleteByIdBox;
            set => Set(ref _deleteByIdBox, value);
        }


        private string _deletedLicenseBox = string.Empty;
        public string DeletedLicenseBox
        {
            get => _deletedLicenseBox;
            set => Set(ref _deletedLicenseBox, value);
        }


        private string _generatedLicenseBox = string.Empty;
        public string GeneratedLicenseBox
        {
            get => _generatedLicenseBox;
            set => Set(ref _generatedLicenseBox, value);
        }


        private bool _generateLicenseButtonEnabled;
        public bool GenerateLicenseButtonEnabled
        {
            get => _generateLicenseButtonEnabled;
            set => Set(ref _generateLicenseButtonEnabled, value);
        }


        private List<GitLabGroup> _groups;
        public List<GitLabGroup> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                NotifyOfPropertyChange(() => Groups);
            }
        }

        private GitLabGroup _selectedGroup;
        public GitLabGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                NotifyOfPropertyChange(() => SelectedGroup);

                ValidateCreateButton();
            }
        }


        private CollaborationConfiguration _collaborationConfig = new();
        public CollaborationConfiguration CollaborationConfig
        {
            get => _collaborationConfig;
            set
            {
                _collaborationConfig = value;
                NotifyOfPropertyChange(() => CollaborationConfig);
            }
        }


        private string _generatedGitLabLicense;
        public string GeneratedGitLabLicense
        {
            get => _generatedGitLabLicense;
            set => Set(ref _generatedGitLabLicense, value);
        }


        private string _generateLicenseMessage;
        public string GenerateLicenseMessage
        {
            get => _generateLicenseMessage;
            set => Set(ref _generateLicenseMessage, value);
        }

        private Brush _generateLicenseMessageBrush = Brushes.Red;
        public Brush GenerateLicenseMessageBrush
        {
            get => _generateLicenseMessageBrush;
            set => Set(ref _generateLicenseMessageBrush, value);
        }


        private Brush _deleteUserForeColor = Brushes.Red;
        public Brush DeleteUserForeColor
        {
            get => _deleteUserForeColor;
            set => Set(ref _deleteUserForeColor, value);
        }

        private string _deleteUserMessage;
        public string DeleteUserMessage
        {
            get => _deleteUserMessage;
            set => Set(ref _deleteUserMessage, value);
        }


        private Brush _fetchUserForeColor = Brushes.Red;
        public Brush FetchUserForeColor
        {
            get => _fetchUserForeColor;
            set => Set(ref _fetchUserForeColor, value);
        }

        private string _fetchUserMessage;
        public string FetchUserMessage
        {
            get => _fetchUserMessage;
            set => Set(ref _fetchUserMessage, value);
        }




        private List<DashboardUser> _gitlabUsers;
        public List<DashboardUser> DashboardUsers
        {
            get { return _gitlabUsers; }
            set => Set(ref _gitlabUsers, value);
        }


        private User _dashboardUser;
        public User DashboardUser
        {
            get { return _dashboardUser; }
            set => Set(ref _dashboardUser, value);
        }


        private List<GitUser> _gitLabUsers;
        public List<GitUser> GitLabUsers
        {
            get => _gitLabUsers;
            set => Set(ref _gitLabUsers, value);
        }



        private string _paratextUserName = string.Empty;
        public string ParatextUserName
        {
            get { return _paratextUserName; }
            set => Set(ref _paratextUserName, value);
        }


        private string _fetchedGitLabLicense;
        public string FetchedGitLabLicense
        {
            get => _fetchedGitLabLicense;
            set => Set(ref _fetchedGitLabLicense, value);
        }



        private CollaborationConfiguration _collabUser;
        public CollaborationConfiguration CollabUser
        {
            get => _collabUser;
            set => Set(ref _collabUser, value);
        }

        public ICollectionView ProjectUserConnectionsCollectionView { get; set; }

        private List<ProjectUserConnection> _projectUserConnections;
        public List<ProjectUserConnection> ProjectUserConnections
        {
            get => _projectUserConnections;
            set => Set(ref _projectUserConnections, value);
        }

        private string _userFilterString = string.Empty;
        public string UserFilterString
        {
            get => _userFilterString;
            set
            {
                _userFilterString = value ?? string.Empty;
                CheckAndRefreshGrid();
            }
        }

        private string _projectFilterString = string.Empty;

        public string ProjectFilterString
        {
            get => _projectFilterString;
            set
            {
                _projectFilterString = value ?? string.Empty;
                CheckAndRefreshGrid();
            }
        }

        #endregion //Observable Properties


        #region Constructor

#pragma warning disable CS8618
        public ShellViewModel()
#pragma warning restore CS8618
        {

            var _collaborationManager =

            _mySqlHttpClientServices = ServiceCollectionExtensions.GetSqlHttpClientServices();

            _gitLabServices = ServiceCollectionExtensions.GetGitLabHttpClientServices();


            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            Title = $"License Key Manager - {thisVersion!.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            EmailChecked = true;
        }

        protected override async void OnViewReady(object view)
        {
            FetchByIdInput = Visibility.Collapsed;
            try
            {
                Groups = await _gitLabServices.GetAllGroups();
            }
            catch
            {
                // ignored
            }

            var dashboardUsersList = await _mySqlHttpClientServices.GetAllDashboardUsers();
            DashboardUsers = dashboardUsersList.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

            var gitUsers = await _gitLabServices.GetAllUsers();
            GitLabUsers = gitUsers.OrderBy(s => s.UserName).ToList();

            await RefreshProjectUserConnectionGrid();

            base.OnViewReady(view);
        }

        #endregion //Constructor


        #region Methods

        public async void GenerateLicense_OnClick()
        {
            GeneratedLicenseBoxText = string.Empty;
            GenerateLicenseMessage = string.Empty;
            GeneratedGitLabLicense = string.Empty;

            var emailAlreadyExists = await CheckForPreExistingDashboardEmail(EmailBox);

            if (emailAlreadyExists)
            {
                GenerateLicenseMessage = "Dashboard Email already exists on system!";
                GenerateLicenseMessageBrush = Brushes.Red;
            }
            else
            {
                var gitlabUsersExists = await CheckForPreExistingGitlabEmail(EmailBox);

                if (gitlabUsersExists)
                {
                    GenerateLicenseMessage = "Gitlab users already exists on system!";
                    GenerateLicenseMessageBrush = Brushes.Red;
                }
                else
                {
                    // create GitLab User First to get it's GitLab Id
                    var gitLabUserId = await CreateGitLabUser();

                    // create Dashboard User
                    var licenseKey = await GenerateDashboardLicense(FirstNameBox, LastNameBox, Guid.NewGuid(), EmailBox, gitLabUserId);
                    GeneratedLicenseBoxText = licenseKey;

                    GenerateLicenseMessage = "Saved to remote server";
                    GenerateLicenseMessageBrush = Brushes.Green;

                    CombinedCreateLicense = CombineLicenses(GeneratedLicenseBoxText, GeneratedGitLabLicense);
                }

            }
        }

        public void GroupSelected()
        {
            // for caliburn
        }

        public async void RefreshDashboardUsersGrid()
        {
            var dashboardUsersList = await _mySqlHttpClientServices.GetAllDashboardUsers();
            DashboardUsers = dashboardUsersList.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
        }

        public async void RefreshGitLabUsersGrid()
        {
            var gitUsers = await _gitLabServices.GetAllUsers();
            GitLabUsers = gitUsers.OrderBy(s => s.UserName).ToList();

        }

        public async Task RefreshProjectUserConnectionGrid()
        {
            var projectUserConnection = new List<ProjectUserConnection>();
            var projects = await _gitLabServices.GetAllProjects();

            foreach (var project in projects)
            {
                var users = await _gitLabServices.GetUsersForProject(null, project.Id);

                foreach (var user in users)
                {
                    projectUserConnection.Add(new ProjectUserConnection
                    {
                        UserName = user.Name,
                        ProjectName = (project.Description ?? "Nameless").ToString(),
                        AccessLevel = user.GetPermissionLevel
                    });
                }
            }
            ProjectUserConnections = projectUserConnection;

            ProjectUserConnectionsCollectionView  = CollectionViewSource.GetDefaultView(ProjectUserConnections);
            ProjectUserConnectionsCollectionView.GroupDescriptions.Clear();
            ProjectUserConnectionsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
            ProjectUserConnectionsCollectionView.Filter += ConnectionCollection_Filter;
        }

        private bool ConnectionCollection_Filter(object obj)
        {
            if (obj is ProjectUserConnection connection)
            {
                if (connection.UserName.ToLower().Contains(UserFilterString.ToLower()) &&
                    connection.ProjectName.ToLower().Contains(ProjectFilterString.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckAndRefreshGrid()
        {
            if (ProjectUserConnections != null && ProjectUserConnectionsCollectionView is not null)
            {
                ProjectUserConnectionsCollectionView.Refresh();
            }
        }

        public void ValidateCreateButton()
        {
            if (SelectedGroup is null)
            {
                return;
            }

            if (FirstNameBox != string.Empty && LastNameBox != string.Empty && EmailBox != string.Empty && SelectedGroup.Name != string.Empty)
            {
                IsCreateButtonEnabled = true;
            }
            else
            {
                IsCreateButtonEnabled = false;
            }


        }


        /// <summary>
        /// Creates the User on the GitLab Server
        /// </summary>
        public async Task<int> CreateGitLabUser()
        {
            var password = GenerateRandomPassword.RandomPassword(16);

            GitLabUser user = await _gitLabServices.CreateNewUser(FirstNameBox, LastNameBox, GetUserName(), password,
                EmailBox, SelectedGroup.Name);

            if (user.Id == 0)
            {
                GenerateLicenseMessage = "Error Creating user on Server";
                GenerateLicenseMessageBrush = Brushes.Red;

                CollaborationConfig = new();
            }
            else
            {
                var accessToken = await _gitLabServices.GeneratePersonalAccessToken(user);

                CollaborationConfig = new CollaborationConfiguration
                {
                    Group = SelectedGroup.Name,
                    RemoteEmail = EmailBox,
                    RemotePersonalAccessToken = accessToken,
                    RemotePersonalPassword = password,
                    RemoteUrl = "",
                    RemoteUserName = user.UserName,
                    UserId = user.Id,
                    NamespaceId = user.NamespaceId,
                };

                _collaborationConfiguration = CollaborationConfig;

                user.Password = password;

                var results = await _mySqlHttpClientServices.CreateNewCollabUser(user, accessToken);

                if (results)
                {
                    GeneratedGitLabLicense = LicenseManager.EncryptCollabJsonToString(CollaborationConfig);
                }

            }

            return user.Id;
        }

        private async Task<bool> CheckForPreExistingDashboardEmail(string email)
        {
            var results = await _mySqlHttpClientServices.GetAllDashboardUsers();

            var dashboardUser = results.FirstOrDefault(du => du.Email == email);

            if (dashboardUser is null)
            {
                return false;
            }

            return true;
        }


        private async Task<bool> CheckForPreExistingGitlabEmail(string email)
        {
            return await _gitLabServices.CheckForExistingUser(GetUserName(), EmailBox);
        }


        private async Task<string> GenerateDashboardLicense(string firstName, string lastName, Guid id, string email,
            int gitLabUserId)
        {
            var licenseUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                IsInternal = IsInternalChecked,
                LicenseVersion = _licenseVersion,
            };

            var encryptedLicense = LicenseManager.EncryptToString(licenseUser);

            var dashboardUser = new DashboardUser
            {
                GitLabUserId = gitLabUserId,
                Email = email,
                ParatextUserName = ParatextUserName,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                LicenseVersion = _licenseVersion,
                LicenseKey = encryptedLicense,
                IsInternal = IsInternalChecked,
                Organization = SelectedGroup.Name,
            };

            var results = await _mySqlHttpClientServices.CreateNewDashboardUser(dashboardUser);

            if (!results)
            {
                encryptedLicense = "User failed to be added to the remote server";
            }

            return encryptedLicense;
        }

        public void DecryptLicense_OnClick()
        {
            try
            {
                var json = LicenseManager.DecryptLicenseFromString(LicenseDecryptionBox, isGenerator: true);
                DashboardUser = LicenseManager.DecryptedJsonToUser(json, isGenerator: true);

                if (DashboardUser.Id == Guid.Empty)
                {
                    DashboardUser = new User();
                    CollabUser = LicenseManager.DecryptCollabToConfiguration(LicenseDecryptionBox);
                }
            }
            catch
            {

            }
        }

        public void ByIdRadio_OnCheck()
        {
            FetchByEmailInput = Visibility.Collapsed;
            FetchByIdInput = Visibility.Visible;
        }

        public void ByEmailRadio_OnCheck()
        {
            FetchByEmailInput = Visibility.Visible;
            FetchByIdInput = Visibility.Collapsed;
        }

        public async void FetchLicenseById_OnClick()
        {
            FetchedEmailBox = string.Empty;
            FetchedLicenseBox = string.Empty;
            FetchedGitLabLicense = string.Empty;
            FetchUserMessage = string.Empty;
            CombinedLicense = string.Empty;

            var fetchByEmail = !IdChecked;

            DashboardUser dashboardUser;
            if (fetchByEmail)
            {
                // Get by Email

                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsByEmail(FetchByEmailBox);

                if (dashboardUser.Id == Guid.Empty)
                {
                    FetchUserForeColor = Brushes.Red;
                    FetchUserMessage = "Email Not Found";
                    return;
                }


                var gitLabUser = await _mySqlHttpClientServices.GetCollabUserExistsByEmail(FetchByEmailBox);
                if (gitLabUser.UserId != -1)
                {
                    var user = new CollaborationConfiguration
                    {
                        UserId = gitLabUser.UserId,
                        RemoteUserName = gitLabUser.RemoteUserName,
                        RemoteEmail = gitLabUser.RemoteEmail,
                        RemotePersonalAccessToken = Encryption.Decrypt(gitLabUser.RemotePersonalAccessToken),
                        RemotePersonalPassword = Encryption.Decrypt(gitLabUser.RemotePersonalPassword),
                        Group = gitLabUser.GroupName,
                        NamespaceId = gitLabUser.NamespaceId,
                    };

                    FetchedGitLabLicense = LicenseManager.EncryptCollabJsonToString(user);
                }
            }
            else
            {
                // get by ID

                Guid.TryParse(FetchByIdBox, out var guid);
                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsById(guid);

                var gitLabUser = await _mySqlHttpClientServices.GetCollabUserExistsByEmail(dashboardUser.Email);
                if (gitLabUser.UserId != -1)
                {
                    var user = new CollaborationConfiguration
                    {
                        UserId = gitLabUser.UserId,
                        RemoteUserName = gitLabUser.RemoteUserName,
                        RemoteEmail = gitLabUser.RemoteEmail,
                        RemotePersonalAccessToken = Encryption.Decrypt(gitLabUser.RemotePersonalAccessToken),
                        RemotePersonalPassword = Encryption.Decrypt(gitLabUser.RemotePersonalPassword),
                        Group = gitLabUser.GroupName,
                        NamespaceId = gitLabUser.NamespaceId,
                    };

                    FetchedGitLabLicense = LicenseManager.EncryptCollabJsonToString(user);
                }
            }

            FetchedEmailBox = dashboardUser.Email ?? string.Empty;
            FetchedLicenseBox = dashboardUser.LicenseKey ?? string.Empty;

            CombinedLicense = CombineLicenses(FetchedLicenseBox, FetchedGitLabLicense);
        }

        private string CombineLicenses(string dashboardLicense, string collabLicense)
        {
            return $"{dashboardLicense}^{collabLicense}";
        }

        public async void DeleteLicenseById_OnClick()
        {

            var deleted = await _mySqlHttpClientServices.DeleteDashboardUserExistsById(Guid.Parse(DeleteByIdBox));

            if (deleted)
            {
                DeletedLicenseBox = DeleteByIdBox;
            }
            else
            {
                DeletedLicenseBox = "Delete failed.";
            }
        }

        public void Copy_OnClick(object sender)
        {
            if (sender is Button button)
            {
                switch (button.Name)
                {
                    case "CombinedLicense":
                        Clipboard.SetText(CombinedLicense);
                        break;
                    case "CopyCombinedLicense":
                        Clipboard.SetText(CombinedCreateLicense);
                        break;
                    case "CopyGeneratedLicense":
                        Clipboard.SetText(GeneratedLicenseBoxText);
                        break;
                    case "CopyGeneratedGitLabLicense":
                        Clipboard.SetText(GeneratedGitLabLicense);
                        break;
                    case "CopyDecryptedFirstName":
                        Clipboard.SetText(DashboardUser.FirstName);
                        break;
                    case "CopyDecryptedLastName":
                        Clipboard.SetText(DashboardUser.LastName);
                        break;
                    case "CopyDecryptedGuid":
                        Clipboard.SetText(DashboardUser.Id.ToString());
                        break;
                    case "CopyDecryptedLicenseVersion":
                        Clipboard.SetText(DashboardUser.LicenseVersion.ToString());
                        break;
                    case "CopyDecryptedCollabConfig":
                        Clipboard.SetText($"UserId: {CollabUser.UserId}\n" +
                                          $"RemoteUserName: {CollabUser.RemoteUserName}\n" +
                                          $"RemoteEmail: {CollabUser.RemoteEmail}\n" +
                                          $"Organization: {CollabUser.Group}\n" +
                                          $"RemotePersonalAccessToken: {CollabUser.RemotePersonalAccessToken}\n" +
                                          $"RemotePersonalPassword: {CollabUser.RemotePersonalPassword}\n" +
                                          $"NamespaceId: {CollabUser.NamespaceId}");
                        break;
                    case "CopyFetchedEmail":
                        Clipboard.SetText(FetchedEmailBox);
                        break;
                    case "CopyFetchedLicense":
                        Clipboard.SetText(FetchedLicenseBox);
                        break;

                    case "CopyFetchedGitLabLicense":
                        Clipboard.SetText(FetchedGitLabLicense);
                        break;
                    case "CopyDeletedLicense":
                        Clipboard.SetText(DeletedLicenseBox);
                        break;
                }
            }
        }

        public void EmailCheckedEvent()
        {
            Console.WriteLine();
        }

        public void IdCheckedEvent()
        {
            Console.WriteLine();
        }


        public async void DeleteLicenseByEmail()
        {

            // get the dashboard user with this email address
            var dashUsers = await _mySqlHttpClientServices.GetAllDashboardUsers();
            var dashUser = dashUsers.FirstOrDefault(s => s.Email == EmailDelete);

            // get the gitlab user on MySQL with this email address
            var gitUsers = await _gitLabServices.GetAllUsers();
            var gitUser = gitUsers.FirstOrDefault(u => u.Email == EmailDelete);

            if (dashUser is null || gitUser is null)
            {
                DeleteUserForeColor = Brushes.Red;
                DeleteUserMessage = $"User {EmailDelete} NOT Found";
            }
            else
            {
                var result = await _mySqlHttpClientServices.DeleteDashboardUserExistsById(dashUser.Id);

                var resultGit = await _mySqlHttpClientServices.DeleteCollaborationUserById(gitUser.Id);

                if (resultGit)
                {
                    var gitLabUsers = await _gitLabServices.GetAllUsers();
                    var gitLabUser = gitLabUsers.FirstOrDefault(u => u.Id == gitUser.Id);
                    if (gitLabUser is not null)
                    {
                        var gitLabProjectUser = new GitLabProjectUser
                        {
                            Id = gitUser.Id,
                        };


                        // remove from GitLab
                        var resultGitLab = await _gitLabServices.DeleteUser(gitLabProjectUser);

                        if (resultGitLab)
                        {
                            DeleteUserForeColor = Brushes.Green;
                            DeleteUserMessage = $"User {dashUser.FullName} Deleted Sucessfully";
                        }
                    }

                }
            }

        }


        #endregion // Methods


    }
}
