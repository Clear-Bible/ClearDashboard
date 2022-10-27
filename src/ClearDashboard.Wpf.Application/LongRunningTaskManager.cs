using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using System.Collections.Concurrent;
using System.Threading;

namespace ClearDashboard.Wpf.Application
{

    public class LongRunningTask
    {
        public LongRunningTask()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Status = ProcessStatus.NotStarted;
        }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public string? Name { get; set; }
        public ProcessStatus Status { get; set; }
    }

    public class LongRunningTaskManager
    {

        private readonly ConcurrentDictionary<string, LongRunningTask>? _tasks;

        public LongRunningTaskManager()
        {
            _tasks = new ConcurrentDictionary<string, LongRunningTask>();
        }

        public bool TryAdd(LongRunningTask longRunningTask)
        {
            longRunningTask.Status = ProcessStatus.Running;
            return _tasks!.TryAdd(longRunningTask.Name!, longRunningTask);
        }

        public bool TryRemove(string taskName, out LongRunningTask? value)
        {
            var result =  _tasks!.TryRemove(taskName, out var task);
            value = task;
            return result;

        }

        public bool HasTask(string taskName)
        {
            return _tasks!.ContainsKey(taskName);
        }

        /// <summary>
        /// Attempts to cancel the task,then removes the task from the wrapped concurrent dictionary. 
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public bool CancelTask(string taskName)
        {
            var result = _tasks.ContainsKey(taskName);
            
            if (result)
            {
                result = _tasks!.TryGetValue(taskName, out var task);

                if (result)
                {
                    var cancellationTokenSource = task!.CancellationTokenSource;
                    if (!cancellationTokenSource!.IsCancellationRequested)
                    {
                        task.Status = ProcessStatus.Canceled;
                        cancellationTokenSource!.Cancel();
                    }

                    result = _tasks.TryRemove(taskName, out _);
                }
            }

            return result;
        }

        public bool TaskComplete(string taskName)
        {
            var result = _tasks!.ContainsKey(taskName);
            if (result)
            {
                result = _tasks.TryRemove(taskName, out _);
            }

            return result;
        }
    }
}
