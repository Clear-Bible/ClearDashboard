using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using ClearApplicationFoundation.Exceptions;
using System.Linq;
using ClearApplicationFoundation.Extensions;
using System;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;


namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class StartupDialogViewModel : WorkflowShellViewModel, IStartupDialog
    {
        private DashboardProjectManager ProjectManager { get; }
        public bool MimicParatextConnection { get; set; }
        private bool _licenseCleared = false;
        private bool _runRegistration = false;

        public StartupDialogViewModel(INavigationService navigationService, ILogger<StartupDialogViewModel> logger, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, DashboardProjectManager projectManager)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;

            CanOk = true;
            DisplayName = "Startup Dialog";
        }

        public StartupDialogViewModel() { }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("Startup", "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency inject registrations of 'IWorkflowStepViewModel' with the key of 'Startup'.  Please check the dependency registration in your bootstrapper implementation.");
            }

#if RELEASE
            _runRegistration = CheckLicense(IoC.Get<RegistrationDialogViewModel>());
#endif

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = _runRegistration ? Steps![0] : Steps![1];

            IsLastWorkflowStep = (Steps.Count == 1);
            EnableControls = true;
            await ActivateItemAsync(CurrentStep, cancellationToken);

            if (MimicParatextConnection)
            {
                var vm = Steps[0] as ProjectPickerViewModel;
                vm.Connected = true;
            }

            if (_runRegistration)
            {
                var vm = Steps[1] as ProjectPickerViewModel;
                if (vm.IsParatextRunning)
                {
                    vm.Connected = true;
                }
                
            }

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

        public bool CheckLicense<TViewModel>(TViewModel viewModel)
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
                            ProjectManager.CurrentUser = new User
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
                        return true;
                    }
                }
                else
                {
                    //MessageBox.Show("Your license key file is missing.");
                    return true;
                }
            }

            return false;
        }

        public object? ExtraData { get; set; }

    }
}
