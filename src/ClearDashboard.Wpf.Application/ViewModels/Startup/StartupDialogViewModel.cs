using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Exceptions;
using System.Linq;
using ClearApplicationFoundation.Extensions;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class StartupDialogViewModel : WorkflowShellViewModel, IStartupDialog
    {
        private bool _licenseCleared = false;
        public StartupDialogViewModel(INavigationService navigationService, ILogger<StartupDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = "Startup Dialog";
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
#if RELEASE
            //ProjectManager.CheckLicense(IoC.Get<RegistrationDialogViewModel>());
            CheckLicense(IoC.Get<RegistrationDialogViewModel>());
#endif
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("Startup", "Order").ToArray();
            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency inject registrations of 'IWorkflowStepViewModel' with the key of 'Startup'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = (Steps.Count == 1);

            EnableControls = true;
            await ActivateItemAsync(Steps[0], cancellationToken);

            await base.OnInitializeAsync(cancellationToken);
        }


        public bool CanCancel => true /* can always cancel */;
        public async void Cancel()
        {
            await TryCloseAsync(false);
        }


        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public void CheckLicense<TViewModel>(TViewModel viewModel)
        {
            if (!_licenseCleared)
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(documentsPath, "ClearDashboard_Projects\\license.txt");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var decryptedLicenseKey = LicenseManager.DecryptFromFile(filePath);
                        var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedLicenseKey);
                        if (decryptedLicenseUser.Id != null)
                        {
                            CurrentUser = new User
                            {
                                FirstName = decryptedLicenseUser.FirstName,
                                LastName = decryptedLicenseUser.LastName,
                                Id = Guid.Parse(decryptedLicenseUser.Id)
                            };

                        }

                        _licenseCleared = true;
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("There was an issue decrypting your license key.");
                        PopupRegistration(viewModel);
                    }
                }
                else
                {
                    //MessageBox.Show("Your license key file is missing.");
                    PopupRegistration(viewModel);
                }
            }
        }

        private void PopupRegistration<TViewModel>(TViewModel viewModel)
        {
            Logger.LogInformation("Registration called.");

            dynamic settings = new ExpandoObject();
            settings.Width = 850;
            settings.WindowStyle = WindowStyle.None;
            settings.ShowInTaskbar = false;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Absolute;
            settings.HorizontalOffset = SystemParameters.FullPrimaryScreenWidth / 2 - 100;
            settings.VerticalOffset = SystemParameters.FullPrimaryScreenHeight / 2 - 50;
            settings.Title = "License Registration";
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;

            var created = _windowManager.ShowDialogAsync(viewModel, null, settings);
            _licenseCleared = true;
        }

        public object? ExtraData { get; set; }

    }
}
