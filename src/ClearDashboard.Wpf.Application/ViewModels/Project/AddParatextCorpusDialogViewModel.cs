using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class AddParatextCorpusDialogViewModel : ValidatingApplicationScreen<AddParatextCorpusDialogViewModel>
    {
        #region Member Variables

        private readonly ILogger<AddParatextCorpusDialogViewModel>? _logger;
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

        private Tokenizers _selectedTokenizers = Tokenizers.LatinWordTokenizer;
        public Tokenizers SelectedTokenizers
        {
            get => _selectedTokenizers;
            set => Set(ref _selectedTokenizers, value);
        }

        public ParatextProjectMetadata? SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);

                CheckUsfm();
                
                ValidationResult = Validator?.Validate(this);
                CanOk = ValidationResult.IsValid;
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
        public AddParatextCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public AddParatextCorpusDialogViewModel(INavigationService? navigationService,
            ILogger<AddParatextCorpusDialogViewModel>? logger,
            DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator,
            IValidator<AddParatextCorpusDialogViewModel> validator, IMediator? mediator, ILifetimeScope? lifetimeScope)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            _logger = logger;
            _projectManager = projectManager;

            ErrorTitle = LocalizationStrings.Get("AddParatextCorpusDialog_NoErrors", _logger);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
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
            
            return base.OnInitializeAsync(cancellationToken);

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

        protected override ValidationResult Validate()
        {
            return (SelectedProject != null) ? Validator?.Validate(this) : null;
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

        public bool CanCancel => true /* can always cancel */;

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        #endregion // Methods
    }
}
