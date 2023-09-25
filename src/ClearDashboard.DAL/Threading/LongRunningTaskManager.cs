using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Threading
{
    public class LongRunningTaskManager : IDisposable
    {
        public CancellationTokenSource CancellationTokenSource;
        public readonly ConcurrentDictionary<string, LongRunningTask?> Tasks;
        private readonly ILogger<LongRunningTaskManager> _logger;

        public LongRunningTaskManager(ILogger<LongRunningTaskManager> logger)
        {
            _logger = logger;
            Tasks = new ConcurrentDictionary<string, LongRunningTask?>();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public void CancelAllTasks()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    CancellationTokenSource.Cancel();
                    return;

                }
                catch (AggregateException ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while canceling a registered tasks.");
                    throw;
                }
            }

            _logger.LogInformation("A previous cancellation was requested.  Ignoring the request.");
        }

        /// <summary>
        /// Attempts to cancel the task,then removes the task from the wrapped concurrent dictionary. 
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool CancelTask(string taskName)
        {
            _logger.LogInformation($"Attempting to cancel task named: {taskName}");
            var result = Tasks.ContainsKey(taskName);

            if (result)
            {
                _logger.LogInformation($"Found task named: {taskName}");
                result = Tasks.TryGetValue(taskName, out var task);

                if (result)
                {
                    var cancellationTokenSource = task!.CancellationTokenSource;
                    if (!cancellationTokenSource!.IsCancellationRequested)
                    {
                        _logger.LogInformation($"Calling cancel on CancellationTokenSource task named: {taskName}.");
                        task.Status = LongRunningTaskStatus.Cancelled;
                        cancellationTokenSource.Cancel();
                    }
                    else
                    {
                        _logger.LogInformation($"CancellationTokenSource on task named: {taskName} has already been called.");
                    }

                    result = TryRemove(taskName, out _);
                    if (result)
                    {
                        _logger.LogInformation($"Removed task named: {taskName}.");
                    }
                }
            }

            return result;
        }

        private void DisposeTasks()
        {
            if (!Tasks.IsEmpty)
            {
                _logger.LogInformation($"Removing the following {Tasks.Count} tasks:");
                foreach (var task in Tasks)
                {
                    _logger.LogInformation($"\tDisposing {task.Key}");
                    task.Value!.Dispose();
                }
                Tasks.Clear();

            }
            else
            {
                _logger.LogInformation($"No tasks currently being managed.");
            }
        }

        public LongRunningTask Create(string taskName, LongRunningTaskStatus status = LongRunningTaskStatus.NotStarted)
        {
            if (!HasTask(taskName))
            {
                var longRunningTask = new LongRunningTask(taskName, CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token), status);
                try
                {
                    Tasks[taskName] = longRunningTask;
                    return longRunningTask;
                }
                catch (Exception ex)
                {
                    throw new LongRunningTaskException("Could not create an instance of LongRunningTask. See inner exception for details.", ex);
                }
            }

            throw new LongRunningTaskException($"A task with the name '{taskName}' already exists.");

        }

        //public bool TryAdd(LongRunningTask longRunningTask)
        //{
        //    longRunningTask.Status = LongRunningTaskStatus.Running;
        //    return _tasks.TryAdd(longRunningTask.Name, longRunningTask);
        //}

        private bool TryRemove(string taskName, out LongRunningTask? value)
        {
            var result = Tasks.TryRemove(taskName, out value);
            value!.Dispose();
            return result;

        }

        public bool HasTask(string taskName)
        {
            Debug.WriteLine($"Checking for task named: {taskName}");

            return Tasks.ContainsKey(taskName);
        }

        public bool HasTasks()
        {
            return Tasks.Any();
        }

        public LongRunningTask GetTask(string taskName)
        {
            return (Tasks[taskName] ?? default)!;
        }

        public bool TaskComplete(string taskName)
        {
            if (HasTask(taskName))
            {
                var result = Tasks.TryRemove(taskName, out var task);
                if (result)
                {
                    task!.Complete();
                    _logger.LogInformation($"Successfully removed task with name '{taskName}';");
                }
            }

            return false;
        }

        public void Dispose()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource.Dispose();
            DisposeTasks();
        }
    }
}
