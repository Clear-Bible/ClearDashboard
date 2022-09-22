using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class TranslationSetStepViewModel : DashboardApplicationWorkflowStepViewModel<ParallelCorpusDialogViewModel>
{
    private bool _canAdd;
    private string _translationSetDisplayName;

    public TranslationSetStepViewModel()
    {

    }

    public TranslationSetStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<TranslationSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanAdd = true;

    }

    public bool CanAdd
    {
        get => _canAdd;
        set => Set(ref _canAdd, value);
    }

    public string TranslationSetDisplayName
    {
        get => _translationSetDisplayName;
        set => Set(ref _translationSetDisplayName, value);
    }


    public async void Add()
    {
        await ParentViewModel!.AddTranslationSet(TranslationSetDisplayName);

        ParentViewModel!.Ok();

    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationStrings.Get("ParallelCorpusDialog_AddTranslationSet", Logger);
        return base.OnActivateAsync(cancellationToken);
    }
}