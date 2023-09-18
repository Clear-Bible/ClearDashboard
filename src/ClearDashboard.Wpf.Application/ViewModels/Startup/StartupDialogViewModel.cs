using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        private ParatextProjectMetadata? _selectedParatextProject;
        public ParatextProjectMetadata? SelectedParatextProject
        {
            get => _selectedParatextProject;
            set => Set(ref _selectedParatextProject, value);
        }

        private ParatextProjectMetadata? _selectedParatextBtProject;
        public ParatextProjectMetadata? SelectedParatextBtProject
        {
            get => _selectedParatextBtProject;
            set => Set(ref _selectedParatextBtProject, value);
        }

        private ParatextProjectMetadata? _selectedParatextLwcProject;
        public ParatextProjectMetadata? SelectedParatextLwcProject
        {
            get => _selectedParatextLwcProject;
            set => Set(ref _selectedParatextLwcProject, value);
        }

        public string ProjectName { get; set; }

        public List<string?> ParatextProjectIds => new() {
            SelectedParatextProject?.Id,
            SelectedParatextBtProject?.Id,
            SelectedParatextLwcProject?.Id};

        private bool _includeBiblicalTexts = true;
        public bool IncludeBiblicalTexts
        {
            get => _includeBiblicalTexts;
            set => Set(ref _includeBiblicalTexts, value);
        }

        private IEnumerable<string>? _selectedBookIds = null;
        public IEnumerable<string>? SelectedBookIds
        {
            get => _selectedBookIds;
            set => Set(ref _selectedBookIds, value);
        }

        public StartupDialogViewModel(SelectedBookManager selectedBookManager, INavigationService navigationService, ILogger<StartupDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope,DashboardProjectManager projectManager)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            SelectedBookManager = selectedBookManager;

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

            var projectTemplateViews = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ProjectTemplate", "Order").ToArray();

            if (projectTemplateViews == null || !projectTemplateViews.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'ProjectTemplate'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            _runRegistration = CheckLicenseToRunRegistration(IoC.Get<RegistrationDialogViewModel>());

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            foreach (var projectTemplateView in projectTemplateViews)
            {
                Steps!.Add(projectTemplateView);
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

        public SelectedBookManager SelectedBookManager { get; internal set; }

        public void Reset()
        {
            // Reset everything in case the wizard is activated again:
            SelectedParatextProject = null;
            SelectedParatextBtProject = null;
            SelectedParatextLwcProject = null;
            IncludeBiblicalTexts = true;
            SelectedBookIds = null;
            SelectedBookManager.UnselectAllBooks();
        }

        public async Task GoToStep(int stepIndex, CancellationToken cancellationToken = default)
        {
            if (stepIndex >= 0 && stepIndex <= Steps!.Count - 1)
            {
                CurrentStep = Steps![stepIndex];
                await ActivateItemAsync(CurrentStep, cancellationToken);
            }
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
