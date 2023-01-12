using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public interface IAquaDialogViewModel : IParatextCorpusDialogViewModel
    {
        Task<LongRunningTaskStatus> RequestAnalysis();

        Visibility StatusBarVisibility { get; set; }
    }
}
