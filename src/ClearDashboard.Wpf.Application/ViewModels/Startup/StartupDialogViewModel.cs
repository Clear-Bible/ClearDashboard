using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class StartupDialogViewModel : WorkflowShellViewModel, IStartupDialog
    {

        #region Member Variables

        private DashboardProjectManager ProjectManager { get; }
        public bool MimicParatextConnection { get; set; }
        public static bool GoToSetup = false;
        public static bool GoToTemplate = false;
        public static bool ProjectAlreadyOpened = false;

        public string Version { get; set; }

        #endregion //Member Variables


        #region Public Properties

        private bool _licenseCleared = false;
        private bool _runRegistration = false;
        public string ProjectName { get; set; }

        public List<string?> ParatextProjectIds => new() {
            SelectedParatextProject?.Id,
            SelectedParatextBtProject?.Id,
            SelectedParatextLwcProject?.Id};

        public SelectedBookManager SelectedBookManager { get; internal set; }
        public object? ExtraData { get; set; }
        public bool CanCancel => true /* can always cancel */;

        #endregion //Public Properties


        #region Observable Properties

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

		private bool _importWordAnalysesParatextProject = false;
		public bool ImportWordAnalysesParatextProject
		{
			get => _importWordAnalysesParatextProject;
			set => Set(ref _importWordAnalysesParatextProject, value);
		}

		private bool _importWordAnalysesParatextBtProject = false;
		public bool ImportWordAnalysesParatextBtProject
		{
			get => _importWordAnalysesParatextBtProject;
			set => Set(ref _importWordAnalysesParatextBtProject, value);
		}

		private bool _importWordAnalysesParatextLwcProject = false;
		public bool ImportWordAnalysesParatextLwcProject
		{
			get => _importWordAnalysesParatextLwcProject;
			set => Set(ref _importWordAnalysesParatextLwcProject, value);
		}

		private bool _includeOtBiblicalTexts = true;
        public bool IncludeOtBiblicalTexts
        {
            get => _includeOtBiblicalTexts;
            set => Set(ref _includeOtBiblicalTexts, value);
        }

        private bool _includeNtBiblicalTexts = true;
        public bool IncludeNtBiblicalTexts
        {
            get => _includeNtBiblicalTexts;
            set => Set(ref _includeNtBiblicalTexts, value);
        }

        private IEnumerable<string>? _selectedBookIds = null;
        public IEnumerable<string>? SelectedBookIds
        {
            get => _selectedBookIds;
            set => Set(ref _selectedBookIds, value);
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public StartupDialogViewModel() 
        {
            // no op
        }

        public StartupDialogViewModel(SelectedBookManager selectedBookManager, INavigationService navigationService, ILogger<StartupDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope,DashboardProjectManager projectManager, ProjectBuilderStatusViewModel backgroundTasksViewModel)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            // reset the background tasks view model
            backgroundTasksViewModel = new ProjectBuilderStatusViewModel();

            ProjectManager = projectManager;
            SelectedBookManager = selectedBookManager;

            CanOk = true;

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly()!.GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            DisplayName = "ClearDashboard " + Version;
        }

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
            else if (GoToTemplate)
            {
                CurrentStep = Steps![3];
                GoToTemplate = false;
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
                await vm.GetProjectsVersion(); // load the projects
                await vm.GetCollabProjects(); // load the collab projects
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

        #endregion //Constructor


        #region Methods

        public void Reset()
        {
            // Reset everything in case the wizard is activated again:
            SelectedParatextProject = null;
            SelectedParatextBtProject = null;
            SelectedParatextLwcProject = null;
            IncludeOtBiblicalTexts = true;
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


        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            foreach (var step in Steps.Cast<ApplicationScreen>())
            {
                await step.DeactivateAsync(close);
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }


        public async void Ok()
        {
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

        #endregion // Methods
    }
}
