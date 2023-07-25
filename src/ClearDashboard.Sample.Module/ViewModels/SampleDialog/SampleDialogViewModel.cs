using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Sample.Module.Services;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Sample.Module.ViewModels.SampleDialog
{
    public class SampleDialogViewModel : DashboardApplicationWorkflowShellViewModel, ISampleDialogViewModel
    {
        internal class TaskNames
        {
            public const string AddVersion = "AddVersion";
            public const string AddRevision = "AddRevision";
            public const string GetRevisions = "GetRevisions";
            public const string GetAssessmentStatuses = "GetAssessmentStatuses";
            public const string GetAssessmentResult = "GetAssessmentResult";
        }

        private readonly TokenizedTextCorpusId tokenizedTextCorpusId_;

        #region Member Variables   

        private readonly ILogger<SampleDialogViewModel>? logger_;
        private readonly DashboardProjectManager? projectManager_;
        private readonly ILifetimeScope? lifetimeScope_;
        private readonly ISampleManager? aquaManager_;
        private readonly ILocalizationService _localization;
        private readonly LongRunningTaskManager? longRunningTaskManager_;
        private LongRunningTask? currentLongRunningTask_;


        #endregion //Member Variables


        #region Public Properties

        public string? SampleId { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private string? dialogTitle_;
        public string? DialogTitle
        {
            get => dialogTitle_;
            set
            {
                dialogTitle_ = value;
                NotifyOfPropertyChange(() => DialogTitle);
            }
        }

        private Visibility statusBarVisibility_ = Visibility.Hidden;
        public Visibility StatusBarVisibility
        {
            get => statusBarVisibility_;
            set
            {
                statusBarVisibility_ = value;
                NotifyOfPropertyChange(() => StatusBarVisibility);
            }
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

        #endregion //Observable Properties


        #region Constructors, initialization, and activation

        public SampleDialogViewModel()
        {
            //_errorTitle = "";
        }

        public SampleDialogViewModel(
            DialogMode dialogMode,
            CorpusNodeViewModel corpusNodeViewModel,
            TokenizedTextCorpusId tokenizedTextCorpusId,

            ISampleManager aquaManager,

            ILogger<SampleDialogViewModel> logger,
            DashboardProjectManager projectManager,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            ILocalizationService localization,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            DialogMode = dialogMode;

            CanOk = true;
            logger_ = logger;
            projectManager_ = projectManager;
            lifetimeScope_ = lifetimeScope;
            aquaManager_ = aquaManager;
            _localization = localization;
            longRunningTaskManager_ = longRunningTaskManager;

            DisplayName = _localization.Get("SampleDialog_DisplayName");

            DialogTitle = "Sample dialog title";

            tokenizedTextCorpusId_ = tokenizedTextCorpusId;
        }

        private string? ObtainSampleId(TokenizedTextCorpusId tokenizedTextCorpusId)
        {
            //look in TokenizedTextCorpus.Metadata for an SampleId, and if there return else null.
            return "AQUAID";
        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            SampleId = ObtainSampleId(tokenizedTextCorpusId_);

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode),
                new NamedParameter("aquaId", SampleId),
            };

            var views = lifetimeScope_?.ResolveKeyedOrdered<IWorkflowStepViewModel>("SampleDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'SampleDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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

        #endregion //Constructor


        #region Methods

        public async void Cancel()
        {
            CancelCurrentTask();

            await TryCloseAsync(false);
        }

        private void CancelCurrentTask()
        {
            if (currentLongRunningTask_ is { Status: LongRunningTaskStatus.Running })
            {
                Logger!.LogInformation($"Cancelling {currentLongRunningTask_.Name}");
                currentLongRunningTask_.Cancel();
            }
        }

        public void OnClose(CancelEventArgs args)
        {
            if (args.Cancel)
            {
                Logger!.LogInformation("OnClose() called with 'Cancel' set to true");
                CancelCurrentTask();
            }
        }
        public async void Ok()
        {
            await TryCloseAsync(true);
        }
        public bool CanCancel => true /* can always cancel */;

        public async Task<LongRunningTaskStatus> AddVersion()
        {
            IsBusy = true;
            var taskName = TaskNames.AddVersion;
            currentLongRunningTask_ = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = currentLongRunningTask_!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");
            try
            {
                StatusBarVisibility = Visibility.Visible;
                currentLongRunningTask_.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Adding version.");
                Logger!.LogInformation($"Adding version.");

                await aquaManager_!.AddVersion(
                    tokenizedTextCorpusId_,
                    new ISampleManager.Version("name", "isoLanguage", "isoScript", "versionAbbreviation"),
                    cancellationToken /*,
                    new DelegateProgress(async status =>
                    {
                        var message =
                            $"Adding version: {status.PercentCompleted:P}";
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                        message);
                        Logger!.LogInformation(message);

                    })*/);

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Adding version complete.");

                Logger!.LogInformation($"Adding version complete.");


            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation($"Adding version - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"Adding version - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while adding version.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                longRunningTaskManager_.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Adding version was canceled.");
                }
                IsBusy = false;
                Message = string.Empty;
                StatusBarVisibility = Visibility.Hidden;
            }
            return currentLongRunningTask_.Status;
        }

        public async Task<LongRunningTaskStatus> AddRevision()
        {
            IsBusy = true;
            var taskName = TaskNames.AddRevision;
            currentLongRunningTask_ = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = currentLongRunningTask_!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");
            try
            {
                StatusBarVisibility = Visibility.Visible;
                currentLongRunningTask_.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Adding revision.");
                Logger!.LogInformation($"Adding revision.");

                await aquaManager_!.AddRevision(
                    tokenizedTextCorpusId_,
                    "",
                    cancellationToken,
                    new DelegateProgress(async status =>
                    {
                        var message =
                            $"Adding revision: {status.PercentCompleted:P}";
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                        message);
                        Logger!.LogInformation(message);

                    }));

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Adding revision complete.");

                Logger!.LogInformation($"Adding revision complete.");


            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation($"Adding revision - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"Adding revision - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while adding revision.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                longRunningTaskManager_.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Adding revision was canceled.");
                }
                IsBusy = false;
                Message = string.Empty;
                StatusBarVisibility = Visibility.Hidden;
            }
            return currentLongRunningTask_.Status;
        }

        #endregion // Methods
    }
}
