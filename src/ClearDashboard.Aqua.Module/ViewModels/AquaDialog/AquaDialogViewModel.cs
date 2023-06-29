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
using Autofac.Features.AttributeFilters;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog
{
    public class AquaDialogViewModel : DashboardApplicationWorkflowShellViewModel, IAquaDialogViewModel
    {
        private readonly ILogger<AquaDialogViewModel>? logger_;
        private readonly LongRunningTaskManager? longRunningTaskManager_;
        private List<LongRunningTask> currentLongRunningTasks_ = new();
        private bool dontStartNewTask = false;

        public bool CanCancel => true;

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

        public AquaDialogViewModel()
        {
        }

        public AquaDialogViewModel(
            DialogMode dialogMode,
            TokenizedTextCorpusId tokenizedTextCorpusId,

            ILogger<AquaDialogViewModel> logger,
            DashboardProjectManager projectManager,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            LongRunningTaskManager longRunningTaskManager,
            [KeyFilter("Aqua")] ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            DialogMode = dialogMode;

            CanOk = true;
            logger_ = logger;

            longRunningTaskManager_ = longRunningTaskManager;

            DisplayName = LocalizationService!.Get("Aqua_DialogTitle");

          

            tokenizedTextCorpusId_ = tokenizedTextCorpusId;
        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DialogTitle = LocalizationService!.Get("Aqua_DialogTitle");

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
        public async void Cancel()
        {
            CancelCurrentTasks();
            await TryCloseAsync(false);
        }
        private void CancelCurrentTasks()
        {
            foreach (var task in currentLongRunningTasks_)
            {
                if (task is { Status: LongRunningTaskStatus.Running })
                {
                    Logger!.LogInformation($"Cancelling {task.Name}");
                    task.Cancel();
                }
            }
        }
        public void OnClosing(CancelEventArgs args)
        {
            dontStartNewTask = true;
            CancelCurrentTasks();
        }
        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        private void BeforeStartDefault()
        {
            IsBusy = true;
            StatusBarVisibility = Visibility.Visible;
        }

        private void AfterEndDefault()
        {
            IsBusy = false;
            //StatusBarVisibility = Visibility.Hidden;
        }

        //FIXME: should the following be put in a base class?
        public async Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName, 
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult,
            System.Action? BeforeStart = null,
            System.Action? AfterEnd = null)
        {
            if (BeforeStart == null)
                BeforeStartDefault();

            Random rnd = new Random();
            int num = rnd.Next(1, 999);
            taskName = $"{num}: {taskName}";

            var longRunningTask = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            currentLongRunningTasks_.Add(longRunningTask);
            var cancellationToken = longRunningTask!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");

            LongRunningTaskStatus returnStatus;
            try
            {
                if (dontStartNewTask)
                    throw new OperationCanceledException();

                longRunningTask.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    longRunningTask.Status,
                    cancellationToken,
                    $"{taskName} running");
                Logger!.LogDebug($"{taskName} started");

                ProcessResult(await awaitableFunction(cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    longRunningTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        longRunningTask.Status,
                        cancellationToken,
                        $"{taskName} canceled.");
                    Logger!.LogDebug($"{taskName} cancelled.");
                }
                else
                {
                    longRunningTask.Status = LongRunningTaskStatus.Completed;
                    await SendBackgroundStatus(
                        taskName,
                        longRunningTask.Status,
                        cancellationToken,
                        $"{taskName} complete");
                    Logger!.LogDebug($"{taskName} complete.");
                }
            }
            catch (OperationCanceledException)
            {
                longRunningTask.Status = LongRunningTaskStatus.Cancelled;
                await SendBackgroundStatus(
                    taskName,
                    longRunningTask.Status,
                    cancellationToken,
                    $"{taskName} cancelled.");
                Logger!.LogDebug($"{taskName}: cancelled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    longRunningTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(
                        taskName,
                        longRunningTask.Status,
                        cancellationToken,
                        $"{taskName} cancelled.");
                    Logger!.LogDebug($"{taskName}: cancelled.");

                }
                else
                {
                    longRunningTask.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                       taskName,
                       longRunningTask.Status,
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
                    longRunningTask.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                        taskName,
                        longRunningTask.Status,
                        cancellationToken,
                        $"{taskName} failed: {ex.Message}.",
                        ex);
                    Logger!.LogError(ex, $"{taskName}: failed: {ex}.");
                }
            }
            finally
            {
                returnStatus = longRunningTask.Status;
                longRunningTaskManager_.TaskComplete(taskName);
                currentLongRunningTasks_.Remove(longRunningTask);

                if (AfterEnd == null)
                    AfterEndDefault();
            }
            return returnStatus;
        }
    }
}
