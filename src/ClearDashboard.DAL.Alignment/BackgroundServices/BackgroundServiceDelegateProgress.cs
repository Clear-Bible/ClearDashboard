using Caliburn.Micro;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Threading;

namespace ClearDashboard.DAL.Alignment.BackgroundServices
{
    /// <summary>
    /// Adapter to allow BackgroundService implementations to use Machine's IProgress
    /// utilities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BackgroundServiceDelegateProgress<T> : ILongRunningProgress<ProgressStatus> where T : BackgroundService
    {
        public string? ProcessName { get; set; }

        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<T> _logger;
        private readonly CancellationToken _cancellationToken;

        public delegate BackgroundServiceDelegateProgress<T> Factory(CancellationToken cancellationToken);

        public BackgroundServiceDelegateProgress(IEventAggregator eventAggregator, ILogger<T> logger, CancellationToken cancellationToken)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
            _cancellationToken = cancellationToken;
        }

        public void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ? 
                string.Format(message, status.PercentCompleted) :
                message;

            SendReport(LongRunningTaskStatus.Running, description, null);
        }

        public void ReportCompleted(string? description = null)
        {
            SendReport(LongRunningTaskStatus.Completed, description, null);
        }

        public void ReportException(Exception exception)
        {
            SendReport(LongRunningTaskStatus.Failed, null, exception);
        }

        public void ReportCancelRequestReceived(string? description = null)
        {
            SendReport(LongRunningTaskStatus.CancellationRequested, description, null);
        }

        private async void SendReport(LongRunningTaskStatus processStatus, string? description, Exception? exception)
        {
            if (exception is not null || description is not null)
            {
                _logger.LogInformation("Background task '{ProcessName}' reports status '{processStatus}' with message '{message}'", ProcessName, processStatus, exception?.Message ?? description);
            }
            else
            {
                _logger.LogInformation("Background task '{ProcessName}' reports status '{processStatus}'", ProcessName, processStatus);
            }

            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = ProcessName ?? string.Empty,
                Description = description ?? string.Empty,
                TaskLongRunningProcessStatus = processStatus,
                ErrorMessage = exception != null ? $"{exception}" : null,
                EndTime = DateTime.Now
            };

            await _eventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), _cancellationToken);            
        }
    }
}
