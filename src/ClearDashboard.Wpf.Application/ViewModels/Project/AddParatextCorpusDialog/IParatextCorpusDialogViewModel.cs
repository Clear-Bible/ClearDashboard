﻿using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public interface IParatextCorpusDialogViewModel
    {

        string? CurrentStepTitle { get; set; }
        string? CurrentProject { get; set; }


        List<IWorkflowStepViewModel>? Steps { get; }
        ParatextProjectMetadata SelectedProject { get; set; }
        Tokenizers SelectedTokenizer { get; set; }
        List<string>? BookIds { get; set; }
        ObservableCollection<UsfmError> UsfmErrors { get; set; }

        void Ok();
        void Cancel();
    }
}
