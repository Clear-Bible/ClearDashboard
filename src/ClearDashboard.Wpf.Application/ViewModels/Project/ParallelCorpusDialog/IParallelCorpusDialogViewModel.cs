using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Enums;
using SIL.Machine.Translation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using SmtAlgorithm = ClearDashboard.Wpf.Application.Models.SmtAlgorithm;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public interface IParallelCorpusDialogViewModel
{
    TopLevelProjectIds TopLevelProjectIds { get; set; }
    CorpusNodeViewModel SourceCorpusNodeViewModel { get; set; }
    CorpusNodeViewModel TargetCorpusNodeViewModel { get; set; }
    ParallelCorpusConnectionViewModel ParallelCorpusConnectionViewModel { get; set; }
    SmtAlgorithm SelectedSmtAlgorithm { get; set; }
    IWordAlignmentModel WordAlignmentModel { get; set; }
    DAL.Alignment.Corpora.ParallelCorpus ParallelTokenizedCorpus { get; set; }
    EngineParallelTextCorpus ParallelTextCorpus { get; set; }
    TranslationSet TranslationSet { get; set; }
    IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }
    AlignmentSet AlignmentSet { get; set; }
    string? CurrentStepTitle { get; set; }
    string? CurrentProject { get; set; }
    bool UseDefaults { get; set; }

    bool IsTrainedSymmetrizedModel { get; set; }

    ParallelProjectType ProjectType { get; set; }
    Task<LongRunningTaskStatus> AddParallelCorpus(string parallelCorpusDisplayName);
    Task<LongRunningTaskStatus> TrainSmtModel(bool? isTrainedSymmetrizedModel);
    Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName);
    Task<LongRunningTaskStatus> AddAlignmentSet(string alignmentSetDisplayName);

    Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken,
        string? description = null, Exception? ex = null);
    List<IWorkflowStepViewModel>? Steps { get; }
    string Message { get; set; }

    void Ok();
    void Cancel();

}