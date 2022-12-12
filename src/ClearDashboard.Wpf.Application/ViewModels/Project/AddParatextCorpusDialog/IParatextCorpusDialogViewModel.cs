using System.Collections.Generic;
using System.Threading.Tasks;
using ClearApplicationFoundation.ViewModels.Infrastructure;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public interface IParatextCorpusDialogViewModel
    {

        string? CurrentStepTitle { get; set; }
        string? CurrentProject { get; set; }


        List<IWorkflowStepViewModel>? Steps { get; }


        void Ok();
        void Cancel();
        Task<object> AddParatextCorpus(string paratextCorpusDisplayName);
    }
}
