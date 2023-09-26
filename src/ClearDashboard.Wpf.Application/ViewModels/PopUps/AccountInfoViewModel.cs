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
using System.Reflection;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class AccountInfoViewModel : DashboardApplicationScreen
    {

        private readonly ParatextProxy _paratextProxy;
        private readonly CollaborationManager _collaborationManager;
        public string VersionInfo { get; set; } = string.Empty;

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


        public AccountInfoViewModel()
        {
            //no-op
        }

        public AccountInfoViewModel(INavigationService navigationService, ILogger<AccountInfoViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService, CollaborationManager collaborationManager,
            ParatextProxy paratextProxy)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            var localizedString = LocalizationService!.Get("AboutView_Version");
            VersionInfo = $"{localizedString}: {thisVersion.ToString()}";

            _collaborationManager = collaborationManager;
            _paratextProxy = paratextProxy;

            var registration = _paratextProxy.GetParatextRegistrationData();
            ParatextUsername =registration.Name;
            ParatextEmail = registration.Email;

            var license = LicenseManager.DecryptLicenseFromFileToUser(LicenseManager.LicenseFilePath);
            ClearDashboardUsername = license.FullName;

            var config = _collaborationManager.GetConfig();
            CollaborationUsername = config.RemoteUserName;
            CollaborationEmail= config.RemoteEmail;
        }

        public async void Close()
        {
            await this.TryCloseAsync();
        }
    }
}
