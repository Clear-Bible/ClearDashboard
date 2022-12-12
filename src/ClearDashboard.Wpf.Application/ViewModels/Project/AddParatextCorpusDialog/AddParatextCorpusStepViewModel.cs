using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class AddParatextCorpusStepViewModel : DashboardApplicationWorkflowStepViewModel<IParatextCorpusDialogViewModel>
    {
        #region Member Variables

        private readonly ILogger<AddParatextCorpusStepViewModel>? _logger;
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

                CheckUsfm();

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

        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        private DialogMode _dialogMode;
        public DialogMode DialogMode
        {
            get => _dialogMode;
            set => Set(ref _dialogMode, value);
        }


        public bool CanCancel => true /* can always cancel */;

        #endregion //Observable Properties


        #region Constructor
        public AddParatextCorpusStepViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public AddParatextCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<SmtModelStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
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


        private bool _canOk;

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        #endregion // Methods

    }
}
