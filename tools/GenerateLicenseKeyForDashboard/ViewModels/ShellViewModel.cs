using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Services;
using HttpClientToCurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.Helpers;

namespace GenerateLicenseKeyForDashboard.ViewModels
{
    public class ShellViewModel : Screen
    {
        // ReSharper disable global MemberCanBePrivate.Global

        #region Member Variables   

        private readonly int _licenseVersion = 2;
        private readonly CollaborationHttpClientServices _mySqlHttpClientServices;
        private readonly HttpClientServices _gitLabServices;


        /// <summary>
        /// Function to generate a user name from first & lastnames
        /// </summary>
        /// <returns></returns>
        private string GetUserName() => (FirstNameBox + "." + LastNameBox).ToLower();

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

            try
            {
                Groups = await _gitLabServices.GetAllGroups();
            }
            catch
            {
                // ignored
            }

            base.OnViewReady(view);
        }

        #endregion //Constructor


        #region Methods


        public void GroupSelected()
        {
            // for caliburn
        }

        public void ValidateCreateButton()
        {
            if (FirstNameBox != string.Empty && LastNameBox != string.Empty && EmailBox != string.Empty && SelectedGroup.Name != string.Empty)
            {
                IsCreateButtonEnabled = true;
            }
            else
            {
                IsCreateButtonEnabled = false;
            }

            
        }


        public async void GenerateLicense_OnClick()
        {
            var emailAlreadyExists = await CheckForPreExistingEmail(EmailBox);

            if (emailAlreadyExists)
            {
                GeneratedLicenseBoxText = "Email already exists";
            }
            else
            {
                // create Dashboard User
                var licenseKey = await GenerateLicense(FirstNameBox, LastNameBox, Guid.NewGuid(), EmailBox);
                GeneratedLicenseBoxText = licenseKey;

                // create GitLab User
                CreateGitLabUser();
            }
        }


        /// <summary>
        /// Creates the User on the GitLab Server
        /// </summary>
        public async void CreateGitLabUser()
        {
            var password = GenerateRandomPassword.RandomPassword(16);

            GitLabUser user = await _gitLabServices.CreateNewUser(FirstNameBox, LastNameBox, GetUserName(), password,
                EmailBox, SelectedGroup.Name).ConfigureAwait(false);

            if (user.Id == 0)
            {
                GeneratedLicenseBoxText = "Error Creating user on Server";

                CollaborationConfig = new();
            }
            else
            {
                var accessToken = await _gitLabServices.GeneratePersonalAccessToken(user).ConfigureAwait(false);

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
                //_collaborationManager.SaveCollaborationLicense(_collaborationConfiguration);

                user.Password = password;

                var results = await _mySqlHttpClientServices.CreateNewUser(user, accessToken).ConfigureAwait(false);

                if (results)
                {
                    GeneratedLicenseBoxText = "Saved to remote server";
                }
                else
                {
                    GeneratedLicenseBoxText = "User already exists on server";
                }

            }

        }

        private async Task<bool> CheckForPreExistingEmail(string email)
        {
            var results = await _mySqlHttpClientServices.GetAllDashboardUsers();

            var dashboardUser = results.FirstOrDefault(du => du.Email == email);

            if (dashboardUser is null)
            {
                return false;
            }

            return true;
        }

        private async Task<string> GenerateLicense(string firstName, string lastName, Guid id, string email)
        {
            var licenseUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                IsInternal = IsInternalChecked,
                LicenseVersion = _licenseVersion
            };

            var encryptedLicense = LicenseManager.EncryptToString(licenseUser);

            var dashboardUser = new DashboardUser(licenseUser, email, encryptedLicense);

            var results = await _mySqlHttpClientServices.CreateNewDashboardUser(dashboardUser);

            if (!results)
            {
                encryptedLicense = "User failed to be added to the remote server";
            }

            return encryptedLicense;
        }

        public void DecryptLicense_OnClick()
        {
            var json = LicenseManager.DecryptLicenseFromString(LicenseDecryptionBox, isGenerator: true);
            var licenseUser = LicenseManager.DecryptedJsonToUser(json, isGenerator: true);

            DecryptedFirstNameBox = licenseUser.FirstName ?? string.Empty;
            DecryptedLastNameBox = licenseUser.LastName ?? string.Empty;
            DecryptedGuidBox = licenseUser.Id.ToString();
            DecryptedInternalCheckBox = (bool)licenseUser.IsInternal!;
            DecryptedLicenseVersionBox = licenseUser.LicenseVersion.ToString() ?? string.Empty;
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
            var fetchByEmail = FetchByEmailInput;

            DashboardUser dashboardUser;
            if (fetchByEmail == Visibility.Visible)
            {
                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsByEmail(FetchByEmailBox);
            }
            else
            {
                Guid.TryParse(FetchByIdBox, out var guid);
                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsById(guid);
            }

            FetchedEmailBox = dashboardUser.Email ?? string.Empty;
            FetchedLicenseBox = dashboardUser.LicenseKey ?? string.Empty;
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
                    case "CopyGeneratedLicense":
                        Clipboard.SetText(GeneratedLicenseBox);
                        break;
                    case "CopyDecryptedFirstName":
                        Clipboard.SetText(DecryptedFirstNameBox);
                        break;
                    case "CopyDecryptedLastName":
                        Clipboard.SetText(DecryptedLastNameBox);
                        break;
                    case "CopyDecryptedGuid":
                        Clipboard.SetText(DecryptedGuidBox);
                        break;
                    case "CopyFetchedEmail":
                        Clipboard.SetText(FetchedEmailBox);
                        break;
                    case "CopyFetchedLicense":
                        Clipboard.SetText(FetchedLicenseBox);
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


        #endregion // Methods


    }
}
