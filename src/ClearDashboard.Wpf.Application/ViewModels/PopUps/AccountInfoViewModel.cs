﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore.Metadata;

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

        private Visibility _currentAccountDetailsVisibility = Visibility.Collapsed;
        public Visibility CurrentAccountDetailsVisibility
        {
            get => _currentAccountDetailsVisibility;
            set
            {
                _currentAccountDetailsVisibility = value;
                NotifyOfPropertyChange(() => CurrentAccountDetailsVisibility);
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

        private string _clearDashboardId = "Unknown";
        public string ClearDashboardId
        {
            get => _clearDashboardId;
            set
            {
                _clearDashboardId = value;
                NotifyOfPropertyChange(() => ClearDashboardId);
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

        private string _collaborationId = "Unknown";
        public string CollaborationId
        {
            get => _collaborationId;
            set
            {
                _collaborationId = value;
                NotifyOfPropertyChange(() => CollaborationId);
            }
        }

        private ObservableCollection<Tuple<User, string, CollaborationConfiguration>> _userAndPathList = new();
        public ObservableCollection<Tuple<User, string, CollaborationConfiguration>> UserAndPathList
        {
            get => _userAndPathList;
            set
            {
                _userAndPathList = value;
                NotifyOfPropertyChange(() => UserAndPathList);
            }
        }

        private Tuple<User, string, CollaborationConfiguration> _selectedUserAndPath;
        public Tuple<User, string, CollaborationConfiguration> SelectedUserAndPath
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
            ClearDashboardId = license.Id.ToString();

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
                NamespaceId = nameSpaceId,
                TokenId = section["TokenId"] is not null ? Convert.ToInt16(section["TokenId"]) : 0
            };

            CollaborationUsername = config.RemoteUserName;
            CollaborationEmail= config.RemoteEmail;
            CollaborationId = config.UserId.ToString();

            if (license.Id == Guid.Empty)
            {
                CurrentAccountDetailsVisibility = Visibility.Collapsed;
            }
            else
            {
                CurrentAccountDetailsVisibility = Visibility.Visible;
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        private async Task<ObservableCollection<Tuple<User, string, CollaborationConfiguration>>> GetUserAndPathList()
        {
            var allMicrosoftFolders = Directory.GetDirectories(LicenseManager.MicrosoftFolderPath);
            var userSecretsFolders = allMicrosoftFolders.Where(name => name.Contains(LicenseManager.UserSecretsDirectoryName));

            UserAndPathList.Clear();
            foreach (var userSecretsFolderPath in userSecretsFolders)
            {
                var licensePath = Path.Combine(userSecretsFolderPath, LicenseManager.LicenseFolderName,
                    LicenseManager.LicenseFileName);
                var user = LicenseManager.DecryptLicenseFromFileToUser(licensePath);
                //what happens when there's no good license in the folder and this decryption fails?

                var collabConfig = new CollaborationConfiguration();

                var collabConfigFilePath = Path.Combine(
                    userSecretsFolderPath,
                    CollaborationManager.UserSecretsId,
                    CollaborationManager.SecretsFileName
                    );

            if (File.Exists(collabConfigFilePath))
                {
                    using (StreamReader r = new StreamReader(collabConfigFilePath))
                    {
                        string json = r.ReadToEnd();
                        dynamic jsonObj = JsonConvert.DeserializeObject(json);
                        var configJson = jsonObj["Collaboration"];
                        collabConfig = JsonConvert.DeserializeObject<CollaborationConfiguration>(configJson.ToString());
                    }
                }
                
                UserAndPathList.Add(Tuple.Create(user, userSecretsFolderPath, collabConfig));
            }

            UserAndPathList= new ObservableCollection<Tuple<User, string, CollaborationConfiguration>>(UserAndPathList.OrderBy(x => x.Item1.Id.ToString()).ToList());

            return UserAndPathList;
        }

        public async void DeactivateCurrentLicense(Tuple<User, string, CollaborationConfiguration> selectedUserAndPath)
        {
            var activeLicense = LicenseManager.DecryptLicenseFromFileToUser(LicenseManager.LicenseFilePath);

            if (selectedUserAndPath != null && activeLicense.Id == selectedUserAndPath.Item1.Id)
            {
                return;
            }

            try
            {
                Directory.Move(
                    LicenseManager.UserSecretsFolderPath,
                    Path.Combine(LicenseManager.MicrosoftFolderPath, $"{LicenseManager.UserSecretsDirectoryName}_{CollaborationEmail}_{activeLicense.Id}"));
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
                        Path.Combine(LicenseManager.DocumentsDirectoryPath, $"{LicenseManager.CollaborationDirectoryName}_{CollaborationEmail}_{activeLicense.Id}"));
            }
            catch(Exception ex)
            {
                //Do I need to now create a Collab Folder?
                Logger.LogInformation("There is no active license.", ex);
            }

            await OnInitializeAsync(CancellationToken.None);
            await GetUserAndPathList();

            CurrentAccountDetailsVisibility = Visibility.Collapsed;
        }

        private async void ActivateSelectedLicense(Tuple<User, string, CollaborationConfiguration> selectedUserAndPath)
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
                    Path.Combine(LicenseManager.DocumentsDirectoryPath, $"{LicenseManager.CollaborationDirectoryName}_{selectedUserAndPath.Item3.RemoteEmail}_{selectedUserAndPath.Item1.Id}"),
                    LicenseManager.CollaborationDirectoryPath);

            }
            catch (Exception ex)
            {
                //Do I need to now create a Collaboration Folder?
                Logger.LogError("Deactivate Current License failed", ex);
            }

            await OnInitializeAsync(CancellationToken.None);
            await GetUserAndPathList();

            CurrentAccountDetailsVisibility = Visibility.Visible;
        }

        public async void SwitchToSelectedLicense(Tuple<User, string, CollaborationConfiguration> selectedUserAndPath)
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

        public async void RestartApplication()
        {
#if RELEASE
            System.Windows.Forms.Application.Restart();
#endif
            System.Windows.Application.Current.Shutdown();
        }
    }
}
