using ClearApplicationFoundation.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpus
{
    public interface IParatextCorpusDialogViewModel
    {

        List<IWorkflowStepViewModel>? Steps { get; }


        void Ok();
        void Cancel();
    }
}
