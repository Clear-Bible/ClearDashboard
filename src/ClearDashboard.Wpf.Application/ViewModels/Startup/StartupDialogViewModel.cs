using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class StartupDialogViewModel : WorkflowShellViewModel, IStartupDialog
    {
        private DashboardProjectManager ProjectManager { get; }
        public bool MimicParatextConnection { get; set; }
        public static bool InitialStartup = true;
        private bool _licenseCleared = false;
        private bool _runRegistration = false;
        public static bool GoToSetup = false;
        public string Version { get; set; }

        public StartupDialogViewModel(INavigationService navigationService, ILogger<StartupDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope,DashboardProjectManager projectManager)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;

            CanOk = true;

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            DisplayName = "ClearDashboard " + Version;
        }

        public StartupDialogViewModel() { }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("Startup", "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'Startup'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            _runRegistration = CheckLicenseToRunRegistration(IoC.Get<RegistrationDialogViewModel>());

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            if (GoToSetup)
            {
                CurrentStep = Steps![2];
                GoToSetup = false;
            }
            else
            {
                CurrentStep = Steps![1];
            }

            CurrentStep = _runRegistration ? Steps![0] : CurrentStep;

            IsLastWorkflowStep = (Steps.Count == 1);
            EnableControls = true;
            await ActivateItemAsync(CurrentStep, cancellationToken);

            if (MimicParatextConnection)
            {
                var vm = Steps[1] as ProjectPickerViewModel;
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
            InitialStartup = false;
            await TryCloseAsync(true);
        }

        public bool CheckLicenseToRunRegistration<TViewModel>(TViewModel viewModel)
        {
            if (!_licenseCleared)
            {
                _licenseCleared = ProjectManager.SetCurrentUserFromLicense();
                return !_licenseCleared;
            }
            return false;
        }

        public object? ExtraData { get; set; }

    }
}
