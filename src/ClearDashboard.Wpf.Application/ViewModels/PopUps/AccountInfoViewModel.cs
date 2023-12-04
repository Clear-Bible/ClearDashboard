using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Configuration;
using Autofac.Configuration;
using System.Windows.Forms;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class AccountInfoViewModel : DashboardApplicationScreen
    {

        private readonly ParatextProxy _paratextProxy;
        private readonly CollaborationManager _collaborationManager;
        public string VersionInfo { get; set; } = string.Empty;
        
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }

        private User _originalLicense = new();

        private User _currentLicense = new();
        public User CurrentLicense
        {
            get => _currentLicense;
            set
            {
                _currentLicense = value;
                NotifyOfPropertyChange(() => CurrentLicense);

                UserAndPathListVisibility = UserAndPathList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                CloseWindowVisibility = _originalLicense.Id == _currentLicense.Id ? Visibility.Visible : Visibility.Collapsed;
                CloseApplicationVisibility = CloseWindowVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            }
        }

        private Visibility _userAndPathListVisibility = Visibility.Collapsed;
        public Visibility UserAndPathListVisibility
        {
            get => _userAndPathListVisibility;
            set
            {
                _userAndPathListVisibility = value;
                NotifyOfPropertyChange(() => UserAndPathListVisibility);
            }
        }

        private Visibility _closeWindowVisibility = Visibility.Visible;
        public Visibility CloseWindowVisibility
        {
            get => _closeWindowVisibility;
            set
            {
                _closeWindowVisibility = value;
                NotifyOfPropertyChange(() => CloseWindowVisibility);
            }
        }

        private Visibility _closeApplicationVisibility = Visibility.Collapsed;
        public Visibility CloseApplicationVisibility
        {
            get => _closeApplicationVisibility;
            set
            {
                _closeApplicationVisibility = value;
                NotifyOfPropertyChange(() => CloseApplicationVisibility);
            }
        }

        private string _paratextUsername = "Unknown";
        public string ParatextUsername
        {
            get => _paratextUsername;
            set
            {
                _paratextUsername = value;
                NotifyOfPropertyChange(() => ParatextUsername);
            }
        }

        private string _paratextEmail = "Unknown";
        public string ParatextEmail
        {
            get => _paratextEmail;
            set
            {
                _paratextEmail = value;
                NotifyOfPropertyChange(() => ParatextEmail);
            }
        }

        private string _clearDashboardUsername = "Unknown";
        public string ClearDashboardUsername
        {
            get => _clearDashboardUsername;
            set
            {
                _clearDashboardUsername = value;
                NotifyOfPropertyChange(() => ClearDashboardUsername);
            }
        }

        private string _clearDashboardEmail = "Unknown";
        public string ClearDashboardEmail
        {
            get => _clearDashboardEmail;
            set
            {
                _clearDashboardEmail = value;
                NotifyOfPropertyChange(() => ClearDashboardEmail);
            }
        }

        private string _collaborationUsername = "Unknown";
        public string CollaborationUsername
        {
            get => _collaborationUsername;
            set
            {
                _collaborationUsername = value;
                NotifyOfPropertyChange(() => CollaborationUsername);
            }
        }

        private string _collaborationEmail = "Unknown";
        public string CollaborationEmail
        {
            get => _collaborationEmail;
            set
            {
                _collaborationEmail = value;
                NotifyOfPropertyChange(() => CollaborationEmail);
            }
        }

        private ObservableCollection<Tuple<User, string>> _userAndPathList = new();
        public ObservableCollection<Tuple<User, string>> UserAndPathList
        {
            get => _userAndPathList;
            set
            {
                _userAndPathList = value;
                NotifyOfPropertyChange(() => UserAndPathList);
            }
        }

        private Tuple<User, string> _selectedUserAndPath;
        public Tuple<User, string> SelectedUserAndPath
        {
            get => _selectedUserAndPath;
            set
            {
                if (value != null)
                {
                    SwitchToSelectedLicense(value);
                }

                _selectedUserAndPath = value;
                NotifyOfPropertyChange(() => SelectedUserAndPath);
            }
        }
        public AccountInfoViewModel()
        {
            //no-op
        }

        public AccountInfoViewModel(INavigationService navigationService, ILogger<AccountInfoViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService, CollaborationManager collaborationManager,
            ParatextProxy paratextProxy)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            IsEnabled = false;
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            var localizedString = LocalizationService!.Get("AboutView_Version");
            VersionInfo = $"{localizedString}: {thisVersion.ToString()}";

            _collaborationManager = collaborationManager;
            _paratextProxy = paratextProxy;

            OnInitializeAsync(CancellationToken.None);
            _originalLicense = _currentLicense;

            GetUserAndPathList();
            IsEnabled = true;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var registration = _paratextProxy.GetParatextRegistrationData();
            ParatextUsername =registration.Name;
            ParatextEmail = registration.Email;

            var license = LicenseManager.DecryptLicenseFromFileToUser(LicenseManager.LicenseFilePath);
            CurrentLicense = license;
            ClearDashboardUsername = license.FullName;

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<Bootstrapper>();
            var iConfigRoot = configBuilder.Build();
            var section = iConfigRoot.GetSection("Collaboration");

            int userId;
            int nameSpaceId;
            try
            {
                userId = Convert.ToInt16(section["userId"]);
                nameSpaceId = Convert.ToInt16(section["NamespaceId"]);
            }
            catch (Exception)
            {
                userId = 2;
                nameSpaceId = 0;
            }
            var config = new CollaborationConfiguration()
            {
                RemoteUrl = section["RemoteUrl"],
                RemoteEmail = section["RemoteEmail"],
                RemoteUserName = section["RemoteUserName"],
                RemotePersonalAccessToken = section["RemotePersonalAccessToken"],
                Group  = section["Group"],
                RemotePersonalPassword = section["RemotePersonalPassword"],
                UserId = userId,
                NamespaceId = nameSpaceId
            };

            CollaborationUsername = config.RemoteUserName;
            CollaborationEmail= config.RemoteEmail;

            return base.OnInitializeAsync(cancellationToken);
        }

        private async Task<ObservableCollection<Tuple<User, string>>> GetUserAndPathList()
        {
            var allMicrosoftFolders = Directory.GetDirectories(LicenseManager.MicrosoftFolderPath);
            var userSecretsFolders = allMicrosoftFolders.Where(name => name.Contains(LicenseManager.UserSecretsDirectoryName));

            UserAndPathList.Clear();
            foreach (var userSecretsFolderPath in userSecretsFolders)
            {
                var licensePath = Path.Combine(userSecretsFolderPath, LicenseManager.LicenseFolderName, LicenseManager.LicenseFileName);
                var user = LicenseManager.DecryptLicenseFromFileToUser(licensePath);
                //what happens there is no good license in the folder and this decryption fails?
                UserAndPathList.Add(Tuple.Create(user, userSecretsFolderPath));
            }

            UserAndPathList= new ObservableCollection<Tuple<User, string>>(UserAndPathList.OrderBy(x => x.Item1.Id.ToString()).ToList());

            return UserAndPathList;
        }

        public async void DeactivateCurrentLicense(Tuple<User, string> selectedUserAndPath)
        {
            IsEnabled = false;
            //pop up confirmation dialog

            var activeLicense = LicenseManager.DecryptLicenseFromFileToUser(LicenseManager.LicenseFilePath);

            if (selectedUserAndPath != null && activeLicense.Id == selectedUserAndPath.Item1.Id)
            {
                return;
            }

            try
            {
                Directory.Move(
                    LicenseManager.UserSecretsFolderPath,
                    Path.Combine(LicenseManager.MicrosoftFolderPath, $"{LicenseManager.UserSecretsDirectoryName}_{activeLicense.Id}"));
            }
            catch (Exception ex)
            {
                //Do I need to now create a UserSecrets Folder?
                Logger.LogError("Deactivate Current License failed", ex);
            }

            try
            {
                Directory.Move(
                        LicenseManager.CollaborationDirectoryPath,
                        Path.Combine(LicenseManager.DocumentsDirectoryPath, $"{LicenseManager.CollaborationDirectoryName}_{activeLicense.Id}"));
            }
            catch(Exception ex)
            {
                Logger.LogInformation("There is no active license.", ex);
            }

            await OnInitializeAsync(CancellationToken.None);
            await GetUserAndPathList();
            
            IsEnabled = true;
        }

        private async void ActivateSelectedLicense(Tuple<User, string> selectedUserAndPath)
        {
            try
            {
                Directory.Move(
                    selectedUserAndPath.Item2,
                    LicenseManager.UserSecretsFolderPath);
            }
            catch (Exception ex)
            {
                //Do I need to now create a UserSecrets Folder?
                Logger.LogError("Deactivate Current License failed", ex);
            }

            try
            {
                Directory.Move(
                    Path.Combine(LicenseManager.DocumentsDirectoryPath, $"{LicenseManager.CollaborationDirectoryName}_{selectedUserAndPath.Item1.Id}"),
                    LicenseManager.CollaborationDirectoryPath);

            }
            catch (Exception ex)
            {
                //Do I need to now create a Collaboration Folder?
                Logger.LogError("Deactivate Current License failed", ex);
            }

            await OnInitializeAsync(CancellationToken.None);
            await GetUserAndPathList();
        }

        public async void SwitchToSelectedLicense(Tuple<User, string> selectedUserAndPath)
        {
            IsEnabled = false;
            DeactivateCurrentLicense(selectedUserAndPath);

            ActivateSelectedLicense(selectedUserAndPath);

            await OnInitializeAsync(CancellationToken.None);
            IsEnabled = true;
        }

        public async void Close()
        {
            await this.TryCloseAsync();
        }

        public async void CloseApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
