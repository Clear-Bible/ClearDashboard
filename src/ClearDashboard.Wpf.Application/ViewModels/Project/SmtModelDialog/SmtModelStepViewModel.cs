﻿namespace ClearDashboard.Wpf.Application.ViewModels.Project.SmtModelDialog
{
    //public class SmtModelStepViewModel : DashboardApplicationWorkflowStepViewModel<SmtModelDialogViewModel>
    //{

    //    public SmtModelStepViewModel()
    //    {

    //    }

    //    public SmtModelStepViewModel(DashboardProjectManager projectManager,
    //        INavigationService navigationService, ILogger<SmtModelStepViewModel> logger,
    //        IEventAggregator eventAggregator,
    //        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
    //        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    //    {

    //        CanMoveForwards = true;
    //        CanMoveBackwards = true;
    //        EnableControls = true;
    //        CanOk = true;
    //    }

    //    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    //    {
    //        SelectedSmtAlgorithm = SmtAlgorithm.FastAlign;
    //        return base.OnInitializeAsync(cancellationToken);
    //    }

    //    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    //    {
    //        ParentViewModel.CurrentStepTitle =
    //            LocalizationStrings.Get("ParallelCorpusDialog_TrainSmtModel", Logger);
    //        return base.OnActivateAsync(cancellationToken);
    //    }

    //    private bool _canOk;
    //    private SmtAlgorithm _selectedSmtAlgorithm;

    //    public bool CanOk
    //    {
    //        get => _canOk;
    //        set => Set(ref _canOk, value);
    //    }

    //    public void Ok()
    //    {
    //        ParentViewModel?.Ok();
    //    }

    //    public SmtAlgorithm SelectedSmtAlgorithm
    //    {
    //        get => _selectedSmtAlgorithm;
    //        set => Set(ref _selectedSmtAlgorithm, value);
    //    }
    //}
}
