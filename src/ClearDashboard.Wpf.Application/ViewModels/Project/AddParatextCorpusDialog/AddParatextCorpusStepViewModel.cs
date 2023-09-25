using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{

    public class UsfmErrorsWrapper
    {
        public string ProjectName { get; set; }

        public string ProjectId { get; set; }

        public string ErrorTitle { get; set; }
        public ObservableCollection<UsfmError> UsfmErrors { get; set; } = new ObservableCollection<UsfmError>();

        public bool HasUsfmErrors => UsfmErrors.Count > 0;

    }
    public interface IUsfmErrorHost
    {
        List<UsfmErrorsWrapper> UsfmErrorsByProject { get; }

        string GetFormattedUsfmErrors();

        string GetUsfmErrorsFileName();
    }
    public class AddParatextCorpusStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, AddParatextCorpusStepViewModel>, IUsfmErrorHost
    {
        #region Member Variables

        private readonly string _initialParatextProjectId;

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

        private bool _isEnabledSelectedProject = true;
        public bool IsEnabledSelectedProject
        {
            get => _isEnabledSelectedProject;
            set
            {
                _isEnabledSelectedProject = value;
                NotifyOfPropertyChange(() => IsEnabledSelectedProject);
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
            }
        }


        private ObservableCollection<UsfmError> _usfmErrors = new();
        public ObservableCollection<UsfmError> UsfmErrors
        {
            get => _usfmErrors;
            set => Set(ref _usfmErrors, value);
        }


        private List<UsfmErrorsWrapper> _usfmErrorsByProject;
        public List<UsfmErrorsWrapper> UsfmErrorsByProject
        {
            get => _usfmErrorsByProject;
            set => Set(ref _usfmErrorsByProject, value);
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

        public AddParatextCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager, string initialParatextProjectId,
            INavigationService navigationService, ILogger<AddParatextCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope,
            IValidator<AddParatextCorpusStepViewModel> validator,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator,localizationService)
        {
            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            _initialParatextProjectId = initialParatextProjectId;
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
            var result = await ProjectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success)
            {
                var projectDesignSurface = LifetimeScope.Resolve<ProjectDesignSurfaceViewModel>();
                
                var corpusIds = await DAL.Alignment.Corpora.Corpus.GetAllCorpusIds(Mediator);

                List<string> currentNodeIds = new();

                foreach (var corpusId in corpusIds)
                {
                    var currentlyRunning = projectDesignSurface.BackgroundTasksViewModel
                        .CheckBackgroundProcessForTokenizationInProgressIgnoreCompletedOrFailedOrCancelled(corpusId.Name);
                    if (currentlyRunning)
                    {
                        currentNodeIds.Add(corpusId.ParatextGuid);
                    }
                }

                var currentNodes = projectDesignSurface.DesignSurfaceViewModel.CorpusNodes;
                currentNodes.ToList().ForEach(n => currentNodeIds.Add(n.ParatextProjectId));

                if (Projects == null)
                {
                    Projects = result.Data.Where(c => !currentNodeIds.Contains(c.Id)).OrderBy(p => p.Name).ToList();
                }

                if (!string.IsNullOrEmpty(_initialParatextProjectId))
                {
                    SelectedProject = Projects.FirstOrDefault(p => p.Id == _initialParatextProjectId);
                    if (SelectedProject != null)
                    {
                        IsEnabledSelectedProject = false;
                    }
                }
                
                // send new metadata results to the Main Window    
                await EventAggregator.PublishOnUIThreadAsync(new ProjectsMetadataChangedMessage(result.Data), cancellationToken);
            }
            
            await base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor
        

        #region Methods

        public string GetUsfmErrorsFileName()
        {
            return $"{SelectedProject.LongName}-USFM-Errors.txt";
        }
        public string GetFormattedUsfmErrors()
        {
            var sb = new StringBuilder();

            foreach (var u in UsfmErrorsByProject)
            {
                if (u.UsfmErrors.Count > 0)
                {
                    sb.AppendLine($"Project Name: {u.ProjectName}, number of errors: {u.UsfmErrors.Count} ");

                    foreach (var error in u.UsfmErrors)
                    {
                        sb.AppendLine($"\t{error.Reference}\t{error.Error}");
                    }

                    sb.AppendLine();
                }

            }

            return sb.ToString();
        }

        public async void ProjectSelected()
        {
            CanOk = false;

            await CheckUsfm(ParentViewModel);

            ValidationResult = Validator?.Validate(this);
            CanOk = ValidationResult.IsValid;
        }

        private async Task CheckUsfm(IParatextCorpusDialogViewModel? parentViewModel)
        {
            if (SelectedProject is null)
            {
                return;
            }

            ShowSpinner = Visibility.Visible;

            var result = await ProjectManager!.ExecuteRequest(new GetCheckUsfmQuery(SelectedProject!.Id), CancellationToken.None);
            if (result.Success)
            {
                var errors = result.Data;

                if (errors.NumberOfErrors == 0)
                {
                    UsfmErrors = new();
                   
                    ErrorTitle = LocalizationService!.Get("AddParatextCorpusDialog_NoErrors");
                }
                else
                {
                    UsfmErrors = new ObservableCollection<UsfmError>(errors.UsfmErrors);
                    ErrorTitle = LocalizationService!.Get("AddParatextCorpusDialog_ErrorCount");
                }

                UsfmErrorsByProject = new List<UsfmErrorsWrapper>()
                {
                   new() 
                   {
                       ProjectName = SelectedProject!.LongName!,
                       ProjectId = SelectedProject!.Id,
                       UsfmErrors = UsfmErrors,
                       ErrorTitle = ErrorTitle
                   }
                };

                parentViewModel.UsfmErrors = UsfmErrors;

            }

            ShowSpinner = Visibility.Collapsed;
        }


        private bool _canOk;

        public async void Ok()
        {

            ParentViewModel.SelectedProject = SelectedProject;
            ParentViewModel.SelectedTokenizer = SelectedTokenizer;
            await MoveForwards();
        }


        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        #endregion // Methods

        protected override ValidationResult? Validate()
        {
            // TODO
            //throw new NotImplementedException();
            return null;
        }
    }
}
