using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog
{
    public interface IAquaDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        TokenizedTextCorpusId? TokenizedTextCorpusId { get; set; }
        AquaTokenizedTextCorpusMetadata AquaTokenizedTextCorpusMetadata { get; set; }

        void Ok();
        void Cancel();

        public Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName,
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult);
    }
}
