using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface;
using SIL.Machine.Translation;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public interface IParallelCorpusDialogViewModel
{
    CorpusNodeViewModel SourceCorpusNodeViewModel { get; set; }
    CorpusNodeViewModel TargetCorpusNodeViewModel { get; set; }
    ConnectionViewModel ConnectionViewModel { get; set; }
    SmtModelType SelectedSmtAlgorithm { get; set; }
    IWordAlignmentModel WordAlignmentModel { get; set; }
    DAL.Alignment.Corpora.ParallelCorpus ParallelTokenizedCorpus { get; set; }
    EngineParallelTextCorpus ParallelTextCorpus { get; set; }
    TranslationSet TranslationSet { get; set; }
    IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }
    AlignmentSet AlignmentSet { get; set; }
    string? CurrentStepTitle { get; set; }
    string? CurrentProject { get; set; }
    Task<LongRunningTaskStatus> AddParallelCorpus(string parallelCorpusDisplayName);
    Task<LongRunningTaskStatus> TrainSmtModel();
    Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName);
    Task<LongRunningTaskStatus> AddAlignmentSet(string alignmentSetDisplayName);

    Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken,
        string? description = null, Exception? ex = null);
    List<IWorkflowStepViewModel> Steps { get; }

    void Ok();
    void Cancel();

}