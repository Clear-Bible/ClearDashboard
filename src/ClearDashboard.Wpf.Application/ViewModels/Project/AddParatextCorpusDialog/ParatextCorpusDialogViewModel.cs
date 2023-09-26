using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class ParatextCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParatextCorpusDialogViewModel
    {
        #region Member Variables   

        private readonly ILogger<ParatextCorpusDialogViewModel>? _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly string _initialParatextProjectId;
        private readonly ILifetimeScope _lifetimeScope;
        
        private string? _corpusNameToSelect;

        #endregion //Member Variables


        #region Public Properties

        public List<string> BookIds { get; set; } = new();

        public enum CorpusType
        {
            Manuscript,
            Other
        }

        public string? Parameter { get; set; }


        #endregion //Public Properties


        #region Observable Properties

        private Visibility _showSpinner = Visibility.Collapsed;
        public Visibility ShowSpinner
        {
            get => _showSpinner;
            set
            {
                _showSpinner = value;
                NotifyOfPropertyChange(() => ShowSpinner);
            }
        }

        
        private CorpusSourceType _corpusSourceType;
        public CorpusSourceType CorpusSourceType
        {
            get => _corpusSourceType;
            set => Set(ref _corpusSourceType, value);
        }

        
        private List<ParatextProjectMetadata>? _projects;
        public List<ParatextProjectMetadata>? Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }

        
        private Tokenizers _selectedTokenizer = Tokenizers.LatinWordTokenizer;
        public Tokenizers SelectedTokenizer
        {
            get => _selectedTokenizer;
            set => Set(ref _selectedTokenizer, value);
        }

        private ParatextProjectMetadata? _selectedProject;
        public ParatextProjectMetadata? SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);
            }
        }

        


        private ObservableCollection<UsfmError> _usfmErrors = new();
        public ObservableCollection<UsfmError> UsfmErrors
        {
            get => _usfmErrors;
            set
            {
                _usfmErrors = value;
                NotifyOfPropertyChange(() => UsfmErrors);
            }
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }


        private string _errorTitle;
        public string ErrorTitle
        {
            get => _errorTitle;
            set
            {
                _errorTitle = value;
                NotifyOfPropertyChange(() => ErrorTitle);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public ParatextCorpusDialogViewModel()
        {
            // no-op
        }

        public ParatextCorpusDialogViewModel(DialogMode dialogMode,
            ILogger<ParatextCorpusDialogViewModel> logger,
            DashboardProjectManager? projectManager,
            string initialParatextProjectId,
            IEventAggregator eventAggregator,
//            IValidator<ParatextCorpusDialogViewModel> validator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            CanOk = true;

            // TODO
            DisplayName = LocalizationStrings.Get("ParatextCorpusDialog_ParatextCorpus", Logger!);

            DialogMode = dialogMode;

            _logger = logger;
            _projectManager = projectManager;
            _initialParatextProjectId = initialParatextProjectId;
            _lifetimeScope = lifetimeScope;

            ErrorTitle = LocalizationService!.Get("AddParatextCorpusDialog_NoErrors");

        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            var parameters = new List<Autofac.Core.Parameter> 
            { 
                new NamedParameter("dialogMode", DialogMode),
                new NamedParameter("initialParatextProjectId", _initialParatextProjectId),
                new NamedParameter("isUpdateCorpusDialog", false)
            };
            var views = _lifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("AddParatextCorpusDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'ParatextCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = Steps.Count == 1;

            EnableControls = true;
            await ActivateItemAsync(Steps[0], cancellationToken);

            await base.OnInitializeAsync(cancellationToken);

            if (!string.IsNullOrEmpty(Parameter))
            {
                try
                {
                    var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(Parameter);
                    foreach (var value in values)
                    {
                        _corpusNameToSelect = value.Key;
                        break;
                    }
                }
                catch (Exception)
                {
                    //no-op.
                }
            }

        }


        #endregion //Constructor


        #region Methods

        public void OnClose(CancelEventArgs args)
        {
            if (args.Cancel)
            {
                Logger!.LogInformation("OnClose() called with 'Cancel' set to true");
                //CancelCurrentTask();
            }
        }


        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public bool CanCancel => true /* can always cancel */;

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        #endregion // Methods
    }
}
