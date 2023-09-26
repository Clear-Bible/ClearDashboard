using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class UpdateParatextCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParatextCorpusDialogViewModel
    {
        #region Member Variables   

        private readonly ILogger<ParatextCorpusDialogViewModel> _logger;
        private readonly DashboardProjectManager _projectManager;
        private readonly string _paratextProjectId;
        private readonly ILifetimeScope _lifetimeScope;

        #endregion //Member Variables


        #region Public Properties

        public List<string>? BookIds { get; set; } = new();
        public ObservableCollection<UsfmError> UsfmErrors { get; set; }

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

        // Child dialog view model never sets this property;
        // it is only here to implement IParatextCorpusDialogViewModel 
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

        public UpdateParatextCorpusDialogViewModel()
        {
            // no-op
        }

        public UpdateParatextCorpusDialogViewModel(DialogMode dialogMode,
            ILogger<ParatextCorpusDialogViewModel> logger,
            DashboardProjectManager projectManager,
            string paratextProjectId,
            IEventAggregator eventAggregator,
//            IValidator<UpdateParatextCorpusDialogViewModel> validator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            CanOk = true;
            DialogMode = dialogMode;

            _logger = logger;
            _projectManager = projectManager;
            _paratextProjectId = paratextProjectId;
            _lifetimeScope = lifetimeScope;
            _errorTitle = string.Empty;

            DisplayName = LocalizationService!.Get("ParatextCorpusDialog_ParatextCorpus");
            ErrorTitle = LocalizationService!.Get("AddParatextCorpusDialog_NoErrors");
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await RetrieveParatextProjectMetadata(cancellationToken);

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode),
                new NamedParameter("isUpdateCorpusDialog", true)
            };

            var views = _lifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("UpdateParatextCorpusDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'UpdateParatextCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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
        }

        private async Task RetrieveParatextProjectMetadata(CancellationToken cancellationToken)
        {
            var result = await _projectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success && result.HasData)
            {
                try
                {
                    SelectedProject = result.Data!.FirstOrDefault(b =>
                    {
                        return b.Id == _paratextProjectId!.ToLower().Replace("-", "");
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error retrieving Paratext project metadata");
                }
            }
            else
            {
                throw new InvalidOperationException(result.Message);
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
