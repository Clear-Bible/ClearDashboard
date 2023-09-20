using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Shell
{
    public record ToggleBackgroundTasksVisibilityMessage();

    public class BackgroundTasksViewModel : Screen, IHandle<BackgroundTaskChangedMessage>, IHandle<ToggleBackgroundTasksVisibilityMessage>
    {
        private readonly LongRunningTaskManager? _longRunningTaskManager;
        private readonly ILocalizationService _localizationService;
        private readonly IEventAggregator? _eventAggregator;
        private readonly ILogger<BackgroundTasksViewModel> _logger;
        private readonly TimeSpan _startTimeSpan = TimeSpan.Zero;
        private readonly TimeSpan _periodTimeSpan = TimeSpan.FromSeconds(5);
        private readonly int _completedRemovalSeconds = 45;
        private bool _collapseTasksView;

        private Timer? _timer;
        private bool _cleanUpOldBackgroundTasks;

        public BackgroundTasksViewModel()
        {
          
            // required for design-time
        }

        public BackgroundTasksViewModel(LongRunningTaskManager? longRunningTaskManager, IEventAggregator? eventAggregator, ILogger<BackgroundTasksViewModel> logger, ILocalizationService localizationService)
        {
            _longRunningTaskManager = longRunningTaskManager;
            _eventAggregator = eventAggregator;
            _logger = logger;
            _localizationService = localizationService;

            // setup timer to clean up old background tasks
            _timer = new Timer(TimerElapsed, null, _startTimeSpan, _periodTimeSpan);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _eventAggregator.SubscribeOnUIThread(this);
            return base.OnActivateAsync(cancellationToken);
        }


        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator!.Unsubscribe(this);

            if (_longRunningTaskManager!.HasTasks())
            {
                _longRunningTaskManager.CancelAllTasks();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void TimerElapsed(object? state)
        {
            try
            {
                if (_cleanUpOldBackgroundTasks)
                {
                    CleanUpOldBackgroundTasks();
                }
                else
                {
                    _cleanUpOldBackgroundTasks = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while cleaning up background tasks.");
                //swallow for now
            }
          
        }

        public void CancelTask(BackgroundTaskStatus status)
        {
            // update the task entry to show cancelling
            var backgroundTaskStatus = _backgroundTaskStatuses.FirstOrDefault(t => t.Name == status.Name);
            if (backgroundTaskStatus != null)
            {
                // Cancel the long running task
                var canceled = _longRunningTaskManager.CancelTask(status.Name);

                backgroundTaskStatus.EndTime = DateTime.Now;
                backgroundTaskStatus.TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed;
                backgroundTaskStatus.Description = _localizationService!.Get("BackgroundTasks_TaskCancelled");
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);

                _eventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus));

                ToggleSpinner();
            }

        }


        /// <summary>
        /// Cleanup Background tasks that are completed and don't have errors
        /// </summary>
        private void CleanUpOldBackgroundTasks()
        {
            // auto close task view if nothing is in the queue
            if (_backgroundTaskStatuses.Count == 0)
            {
                if (_collapseTasksView)
                {
                    ShowTaskView = Visibility.Collapsed;
                    ShowPopup = false;
                    _collapseTasksView = false;
                }

                _collapseTasksView = true;
                return;
            }

            var taskRemoved = false;
            var presentTime = DateTime.Now;

            for (var index = _backgroundTaskStatuses.Count - 1; index >= 0; index--)
            {
                var timeSpan = presentTime - _backgroundTaskStatuses[index].EndTime;

                // if completed task remove it
                if (_backgroundTaskStatuses[index].TaskLongRunningProcessStatus == LongRunningTaskStatus.Completed && timeSpan.TotalSeconds > _completedRemovalSeconds)
                {
                    var index1 = index;
                    OnUIThread(() =>
                    {
                        if (index1 < _backgroundTaskStatuses.Count)
                        {
                            _backgroundTaskStatuses.RemoveAt(index1);
                        }
                    });

                    taskRemoved = true;
                }
            }

            if (taskRemoved)
            {
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);
            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            if (message.Status.TaskLongRunningProcessStatus == LongRunningTaskStatus.Completed)
            {
                _longRunningTaskManager.TaskComplete(message.Status.Name);
            }

            var backgroundTaskStatus = message.Status;

            // check for duplicate entries
            var taskExists = false;
            foreach (var status in BackgroundTaskStatuses)
            {
                if (status.Name == backgroundTaskStatus.Name)
                {
                    taskExists = true;

                    status.Description = backgroundTaskStatus.Description;
                    if (backgroundTaskStatus.TaskLongRunningProcessStatus == LongRunningTaskStatus.Failed)
                    {
                        if (backgroundTaskStatus.ErrorMessage.Contains("InvalidParameterEngineException"))
                        {
                            status.Description = backgroundTaskStatus.Name+ " Failed:\n " + "An unfitting tokenizer was used for the selected corpus.  For example, the ZWSP tokenizer for the NIV84 corpus.";
                        }
                        else
                        {
                            status.Description = backgroundTaskStatus.Name+ " Failed:\n " + backgroundTaskStatus.ErrorMessage;
                        }
                        ShowPopup = true;
                    }

                    if (backgroundTaskStatus.TaskLongRunningProcessStatus == LongRunningTaskStatus.CancellationRequested)
                    { 
                        CancelTask(backgroundTaskStatus);
                        backgroundTaskStatus.TaskLongRunningProcessStatus = LongRunningTaskStatus.Cancelled;
                    }
                    status.TaskLongRunningProcessStatus = backgroundTaskStatus.TaskLongRunningProcessStatus;

                    NotifyOfPropertyChange(() => BackgroundTaskStatuses);
                    break;
                }
            }

            if (taskExists == false)
            {
                BackgroundTaskStatuses.Add(backgroundTaskStatus);
            }

            ToggleSpinner();

            await Task.CompletedTask;
        }

        private Visibility _showTaskView = Visibility.Collapsed;
        public Visibility ShowTaskView
        {
            get => _showTaskView;
            set => Set(ref _showTaskView, value);
        }

        private ObservableCollection<BackgroundTaskStatus> _backgroundTaskStatuses = new();
        public ObservableCollection<BackgroundTaskStatus> BackgroundTaskStatuses
        {
            get => _backgroundTaskStatuses;
            set => Set(ref _backgroundTaskStatuses, value);
        }

        private Visibility _showSpinner = Visibility.Collapsed;
        private bool _showPopup;

        public Visibility ShowSpinner
        {
            get => _showSpinner;
            set => Set(ref _showSpinner, value);
        }

        public void CloseTaskBox()
        {
            ShowTaskView = Visibility.Collapsed;
            ShowPopup = false;
        }


        private void ToggleSpinner()
        {
            // check to see if all are completed so we can turn off spinner
            var runningTasks = BackgroundTaskStatuses
                .Where(p => p.TaskLongRunningProcessStatus == LongRunningTaskStatus.Running).ToList();
            ShowSpinner = runningTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }


        public bool ShowPopup
        {
            get => _showPopup;
            set => Set(ref _showPopup, value);
        }

        public async  Task HandleAsync(ToggleBackgroundTasksVisibilityMessage message, CancellationToken cancellationToken)
        {
            ShowTaskView = ShowTaskView == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            OnUIThread(() =>
            {
                ShowPopup = !ShowPopup;
            });
           await Task.CompletedTask;
        }

        /// <summary>
        /// Get the number of running tasks that are flagged as high performance mode
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfPerformanceTasksRemaining()
        {
           return BackgroundTaskStatuses
               .Where(x => x.BackgroundTaskType == BackgroundTaskStatus.BackgroundTaskMode.PerformanceMode
               && x.TaskLongRunningProcessStatus == LongRunningTaskStatus.Running)
               .ToList().Count;
        }



        /// <summary>
        /// Check to see if there is a background task for tokenization already in progress
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool CheckBackgroundProcessForTokenizationInProgress(string nodeName)
        {
            var tasks = BackgroundTaskStatuses.Where(x =>
            {
                if (x.Name == MaculaCorporaNames.HebrewCorpusName && nodeName == MaculaCorporaNames.HebrewCorpusName)
                {
                    if (x.Description!.Contains(MaculaCorporaNames.HebrewCorpusName))
                    {
                        return true;
                    }

                    return false;
                }
                
                if (x.Name == MaculaCorporaNames.GreekCorpusName && nodeName == MaculaCorporaNames.GreekCorpusName)
                {
                    if (x.Description!.Contains(MaculaCorporaNames.GreekCorpusName))
                    {
                        return true;
                    }

                    return false;
                }

                return x.Name == nodeName;

            }).ToList();

            if (tasks.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool CheckBackgroundProcessForTokenizationInProgressIgnoreCompletedOrFailedOrCancelled(string nodeName)
        {
            var tasks = BackgroundTaskStatuses.Where(x =>
            {
                if (x.Name == MaculaCorporaNames.HebrewCorpusName && nodeName == MaculaCorporaNames.HebrewCorpusName)
                {
                    if (x.Description!.Contains(MaculaCorporaNames.HebrewCorpusName) && 
                        (x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Completed &&
                         x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Failed &&
                         x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Cancelled))
                    {
                        return true;
                    }

                    return false;
                }

                if (x.Name == MaculaCorporaNames.GreekCorpusName && nodeName == MaculaCorporaNames.GreekCorpusName)
                {
                    if (x.Description!.Contains(MaculaCorporaNames.GreekCorpusName) &&
                        (x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Completed &&
                         x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Failed &&
                         x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Cancelled))
                    {
                        return true;
                    }

                    return false;
                }

                return x.Name == nodeName &&
                       (x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Completed &&
                        x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Failed &&
                        x.TaskLongRunningProcessStatus != LongRunningTaskStatus.Cancelled);

            }).ToList();

            if (tasks.Count > 0)
            {
                return true;
            }

            return false;
        }

        public void CopyText(BackgroundTaskStatus status)
        {
            Clipboard.SetText(status.Name + ": " + status.Description);
        }
    }
}
