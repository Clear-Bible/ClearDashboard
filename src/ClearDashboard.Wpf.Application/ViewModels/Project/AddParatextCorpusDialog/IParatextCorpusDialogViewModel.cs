using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public interface IParatextCorpusDialogViewModel
    {

        string? CurrentStepTitle { get; set; }
        string? CurrentProject { get; set; }


        List<IWorkflowStepViewModel>? Steps { get; }
        ParatextProjectMetadata SelectedProject { get; set; }
        List<string>? BookIds { get; set; }


        void Ok();
        void Cancel();
        Task<object> AddParatextCorpus(string paratextCorpusDisplayName);
    }
}
