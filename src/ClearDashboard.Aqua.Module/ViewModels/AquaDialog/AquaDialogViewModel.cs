using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Threading;
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
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Wpf.Application;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog
{
    public class AquaDialogViewModel : DashboardApplicationWorkflowShellViewModel, IAquaDialogViewModel
    {
        #region Member Variables   

        private readonly ILogger<AquaDialogViewModel>? logger_;
        private readonly DashboardProjectManager? projectManager_;
        private readonly IAquaManager? aquaManager_;
        private readonly LongRunningTaskManager? longRunningTaskManager_;
        private LongRunningTask? currentLongRunningTask_;

        #endregion //Member Variables


        #region Public Properties

        private TokenizedTextCorpusId? tokenizedTextCorpusId_;
        public TokenizedTextCorpusId? TokenizedTextCorpusId
        {
            get => tokenizedTextCorpusId_;
            set
            {
                tokenizedTextCorpusId_ = value;
            }
        }

        public Revision? ActiveRevision { get; set; } = null;
        public Assessment? ActiveAssessment { get; set; } = null;

        private AquaTokenizedTextCorpusMetadata? aquaTokenizedTextCorpusMetadata_;
        public AquaTokenizedTextCorpusMetadata? AquaTokenizedTextCorpusMetadata
        {
            get => aquaTokenizedTextCorpusMetadata_;
            set => aquaTokenizedTextCorpusMetadata_ = value;
        }

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
    
        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        #endregion //Observable Properties


        #region Constructors, initialization, and activation

        public AquaDialogViewModel()
        {
            //_errorTitle = "";
        }

        public AquaDialogViewModel(
            DialogMode dialogMode,
            CorpusNodeViewModel corpusNodeViewModel,
            TokenizedTextCorpusId tokenizedTextCorpusId,

            IAquaManager aquaManager,

            ILogger<AquaDialogViewModel> logger,
            DashboardProjectManager projectManager,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            DialogMode = dialogMode;

            CanOk = true;
            logger_ = logger;
            projectManager_ = projectManager;
            aquaManager_ = aquaManager;
            longRunningTaskManager_ = longRunningTaskManager;

            DisplayName = LocalizationService!.Get("AquaDialog_DisplayName");

            DialogTitle = LocalizationService!.Get("AquaDialog_DialogTitle");

            tokenizedTextCorpusId_ = tokenizedTextCorpusId;
        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode),
            };

            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("AquaDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'AquaDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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

        //FIXME: should the following be put in a base class?
        public async Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName, 
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult)
        {
            IsBusy = true;
            currentLongRunningTask_ = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = currentLongRunningTask_!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");
            try
            {
                StatusBarVisibility = Visibility.Visible;

                currentLongRunningTask_.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    currentLongRunningTask_.Status,
                    cancellationToken,
                    $"{taskName} running");
                Logger!.LogDebug($"{taskName} started");

                ProcessResult(await awaitableFunction(cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} canceled.");
                    Logger!.LogDebug($"{taskName} cancelled.");
                }
                else
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Completed;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} complete");
                    Logger!.LogDebug($"{taskName} complete.");
                }
            }
            catch (OperationCanceledException)
            {
                currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                await SendBackgroundStatus(
                    taskName,
                    currentLongRunningTask_.Status,
                    cancellationToken,
                    $"{taskName} cancelled.");
                Logger!.LogDebug($"{taskName}: cancelled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} cancelled.");
                    Logger!.LogDebug($"{taskName}: cancelled.");

                }
                else
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                       taskName,
                       currentLongRunningTask_.Status,
                       cancellationToken,
                       $"{taskName} failed: {ex.Message}.",
                       ex);
                    Logger!.LogError(ex, $"{taskName}: failed: {ex}.");
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} failed: {ex.Message}.",
                        ex);
                    Logger!.LogError(ex, $"{taskName}: failed: {ex}.");
                }
            }
            finally
            {
                longRunningTaskManager_.TaskComplete(taskName);

                IsBusy = false;
                //StatusBarVisibility = Visibility.Hidden;
            }
            return currentLongRunningTask_.Status;
        }

        #endregion // Methods
    }
}
