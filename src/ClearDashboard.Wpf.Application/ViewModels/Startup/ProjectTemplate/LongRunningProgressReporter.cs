using ClearDashboard.DAL.Alignment.BackgroundServices;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;
using SIL.Machine.Utils;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class LongRunningProgressReporter : ILongRunningProgress<ProgressStatus>
    {
        private readonly string _taskName;
        private readonly ProjectTemplateProcessRunner _processRunner;
        private readonly CancellationToken _cancellationToken;
        public LongRunningProgressReporter(string taskName, ProjectTemplateProcessRunner processRunner, CancellationToken cancellationToken)
        {
            _taskName = taskName;
            _processRunner = processRunner;
            _cancellationToken = cancellationToken;
        }

        public async void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ?
                string.Format(message, status.PercentCompleted) :
                message;

            await _processRunner.SendBackgroundStatus(_taskName, LongRunningTaskStatus.Running, _cancellationToken, description, null);
        }

        public async void ReportCompleted(string? description = null)
        {
            await _processRunner.SendBackgroundStatus(_taskName, LongRunningTaskStatus.Completed, _cancellationToken, description, null);
        }

        public async void ReportException(Exception exception)
        {
            await _processRunner.SendBackgroundStatus(_taskName, LongRunningTaskStatus.Failed, _cancellationToken, null, exception);
        }

        public async void ReportCancelRequestReceived(string? description = null)
        {
            await _processRunner.SendBackgroundStatus(_taskName, LongRunningTaskStatus.CancellationRequested, _cancellationToken, description, null);
        }
    }
}
