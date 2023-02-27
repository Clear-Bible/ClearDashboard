using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog
{
    public interface IAquaDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        TokenizedTextCorpusId? TokenizedTextCorpusId { get; set; }
        AquaTokenizedTextCorpusMetadata AquaTokenizedTextCorpusMetadata { get; set; }

        Revision? ActiveRevision { get; set; }
        Assessment? ActiveAssessment { get; set; }

        void Ok();
        void Cancel();

        bool IsBusy { get; set; }

        string? Message { get; set; }
        public Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName,
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult,
            Action? BeforeStart = null,
            Action? AfterEnd = null);
    }
}
