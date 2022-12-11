using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using System.Threading;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using Newtonsoft.Json;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using FluentValidation;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpus
{
    public class ParatextCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParatextCorpusDialogViewModel //, ValidatingApplicationScreen<ParatextCorpusDialogViewModel>
    {
        internal class TaskNames
        {
            public const string PickCorpus = "PickCorpus";
            public const string PickBooks = "PickBooks";
        }

        #region Member Variables   

        private readonly ILogger<ParatextCorpusDialogViewModel>? _logger;
        private readonly DashboardProjectManager? _projectManager;
        private CorpusSourceType _corpusSourceType;
        private List<ParatextProjectMetadata>? _projects;
        private ParatextProjectMetadata? _selectedProject;

        private string? _corpusNameToSelect;

        #endregion //Member Variables


        #region Public Properties

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


        public CorpusSourceType CorpusSourceType
        {
            get => _corpusSourceType;
            set => Set(ref _corpusSourceType, value);
        }

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

        public ParatextProjectMetadata? SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);

                _ = CheckUsfm();

                // TODO
                //ValidationResult = Validator?.Validate(this);
                //CanOk = ValidationResult.IsValid;
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
            IEventAggregator eventAggregator,
            IValidator<ParatextCorpusDialogViewModel> validator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
            // TODO
        //: base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            CanOk = true;

            // TODO
            DisplayName = LocalizationStrings.Get("ParatextCorpusDialog_ParatextCorpus", Logger!);

            DialogMode = dialogMode;

            _logger = logger;
            _projectManager = projectManager;

            ErrorTitle = Helpers.LocalizationStrings.Get("AddParatextCorpusDialog_NoErrors", _logger);

        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            var parameters = new List<Autofac.Core.Parameter> { new NamedParameter("dialogMode", DialogMode) };
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ParallelCorpusDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'ParallelCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            CorpusSourceType = CorpusSourceType.Paratext;
            var result = await _projectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success)
            {
                Projects = result.Data.OrderBy(p => p.Name).ToList();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor


        #region Methods

        public void CopyToClipboard()
        {
            Clipboard.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (var error in UsfmErrors)
            {
                sb.AppendLine($"{error.Reference}\t{error.Error}");
            }

            Clipboard.SetText(sb.ToString());
        }



        private async Task CheckUsfm()
        {
            if (SelectedProject is null)
            {
                return;
            }

            ShowSpinner = Visibility.Visible;

            var result = await _projectManager.ExecuteRequest(new GetCheckUsfmQuery(SelectedProject!.Id), CancellationToken.None);
            if (result.Success)
            {
                var errors = result.Data;

                if (errors.NumberOfErrors == 0)
                {
                    UsfmErrors = new();
                    ErrorTitle = LocalizationStrings.Get("AddParatextCorpusDialog_NoErrors", _logger);
                }
                else
                {
                    UsfmErrors = new ObservableCollection<UsfmError>(errors.UsfmErrors);
                    ErrorTitle = LocalizationStrings.Get("AddParatextCorpusDialog_ErrorCount", _logger);
                }

            }

            ShowSpinner = Visibility.Collapsed;
        }

        // TODO
        //protected override ValidationResult Validate()
        //{
        //    return (SelectedProject != null) ? Validator?.Validate(this) : null;
        //}


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
