using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class AboutViewModel : DashboardApplicationScreen
    {

        public string VersionInfo { get; set; } = string.Empty;

        private Uri _updateUrl = new Uri("https://www.clear.bible");
        public Uri UpdateUrl
        {
            get => _updateUrl;
            set
            {
                _updateUrl = value;
                NotifyOfPropertyChange(() => UpdateUrl);
            }
        }


        public AboutViewModel()
        {
            //no-op
        }

        public AboutViewModel(INavigationService navigationService, ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            var localizedString = LocalizationService!.Get("AboutView_Version");
            VersionInfo = $"{localizedString}: {thisVersion.ToString()}";
        }

        public void ClickLink()
        {
            if (UpdateUrl.AbsoluteUri == "")
            {
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = UpdateUrl.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }


        public async void Close()
        {
            await this.TryCloseAsync();
        }
    }
}
