using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
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

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public class AquaGetCorpusAnalysisDialogViewModel : DashboardApplicationWorkflowShellViewModel, IAquaGetCorpusAnalysisDialogViewModel
    {
        internal class TaskNames
        {
            public const string RequestAnalysis = "RequestAnalysis";
        }

        #region Member Variables   

        private readonly ILogger<AquaRequestCorpusAnalysisDialogViewModel>? logger_;
        private readonly DashboardProjectManager? projectManager_;
        private readonly ILifetimeScope? lifetimeScope_;
        private readonly IAquaManager? aquaManager_;
        private readonly LongRunningTaskManager? longRunningTaskManager_;
        private LongRunningTask? currentLongRunningTask_;
        private string? paratextProjectId_;
        private string? result_;

        #endregion //Member Variables


        #region Public Properties

        private string? requestId_;
        public string? RequestId
        {
            get => requestId_;
            set
            {
                requestId_ = value;
                NotifyOfPropertyChange(() => RequestId);
            }
        }
        private string? analysis_;
        public string? Analysis
        {
            get => analysis_;
            set
            {
                analysis_ = value;
                NotifyOfPropertyChange(() => Analysis);
            }
        }
        public List<string>? BookIds { get; set; } = new();

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

        public Tokenizers SelectedTokenizer
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
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

        public AquaGetCorpusAnalysisDialogViewModel()
        {
        }

        public AquaGetCorpusAnalysisDialogViewModel(
            string paratextProjectId,
            string requestId,
            DialogMode dialogMode,

            ILogger<AquaRequestCorpusAnalysisDialogViewModel> logger,
            DashboardProjectManager projectManager,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            INavigationService navigationService,
            IMediator mediator,
            IAquaManager aquaManager,
            LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            paratextProjectId_ = paratextProjectId;
            RequestId = requestId;
            DialogMode = dialogMode;

            CanOk = true;
            logger_ = logger;
            projectManager_ = projectManager;
            lifetimeScope_ = lifetimeScope;
            aquaManager_ = aquaManager;
            longRunningTaskManager_ = longRunningTaskManager;

            DisplayName = Helpers.LocalizationStrings.Get("AquaRequestCorpusAnalysisDialog_RequestCorupsAnalysis", Logger!);

            DialogTitle = "Get Aqua Analysis Dialog title";
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await RetrieveParatextProjectMetadata(cancellationToken);

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode),
                new NamedParameter("paratextProjectId", paratextProjectId_!),
                new NamedParameter("requestId", RequestId!),
                new NamedParameter("selectBooksStepNextVisible", true)
            };

            var views = lifetimeScope_?.ResolveKeyedOrdered<IWorkflowStepViewModel>("AquaGetCorpusAnalysisDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'AquaGetCorpusAnalysisDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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
            var result = await projectManager_!.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success && result.HasData)
            {
                SelectedProject = result.Data!.FirstOrDefault(b =>
                               b.Id == paratextProjectId_!.Replace("-", "")) ??
                           throw new InvalidOperationException();
            }
            else
            {
                throw new InvalidOperationException(result.Message);
            }
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

        // The user clicked the close button for the dialog.
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


        public async Task<LongRunningTaskStatus> GetAnalysis()
        {
            IsBusy = true;
            var taskName = TaskNames.RequestAnalysis;
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
                    $"Getting analysis.");
                Logger!.LogInformation($"Getting analysis.");

                result_ = await aquaManager_!.GetCorpusAnalysis(
                    "",
                    cancellationToken,
                    new DelegateProgress(async status =>
                    {
                        var message =
                            $"Getting analysis: {status.PercentCompleted:P}";
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                        message);
                        Logger!.LogInformation(message);

                    }));

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Getting analysis complete.");

                Logger!.LogInformation($"Getting analysis complete.");


            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation($"Getting analysis - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"Getting analysis - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while getting analysis.");
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
                        $"Analysis get was canceled.");
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
