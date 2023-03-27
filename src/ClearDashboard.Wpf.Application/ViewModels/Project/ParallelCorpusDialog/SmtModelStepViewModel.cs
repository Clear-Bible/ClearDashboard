using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class SmtModelStepViewModel : DashboardApplicationWorkflowStepViewModel<IParallelCorpusDialogViewModel>
{
    #region Member Variables   

    #endregion //Member Variables


    #region Public Properties

    #endregion //Public Properties


    #region Observable Properties


    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }


    private bool _canTrain;
    public bool CanTrain
    {
        get => _canTrain;
        set => Set(ref _canTrain, value);
    }

    private bool? _isTrainedSymmetrizedModel = false;
    public bool? IsTrainedSymmetrizedModel 
    { 
        get => _isTrainedSymmetrizedModel; 
        set => Set(ref _isTrainedSymmetrizedModel, value);
    }


    #endregion //Observable Properties


    #region Constructor

    public SmtModelStepViewModel()
    {

    }

    public SmtModelStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<SmtModelStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanTrain = true;
    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationService!.Get("ParallelCorpusDialog_TrainSmtModel");

        if (ParentViewModel.UseDefaults)
        {
            await Train(true);
        }

        base.OnActivateAsync(cancellationToken);
    }

    #endregion //Constructor


    #region Methods

    public async void Train()
    {
        await Train(true);
    }


    public async Task Train(object nothing)
    {
        CanTrain = false;
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.TrainSmtModel(IsTrainedSymmetrizedModel);

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        await MoveForwards();
                        break;
                    case LongRunningTaskStatus.Failed:
                    case LongRunningTaskStatus.Cancelled:
                        ParentViewModel.Cancel();
                        break;
                    case LongRunningTaskStatus.NotStarted:
                        break;
                    case LongRunningTaskStatus.Running:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                ParentViewModel!.Cancel();
            }
        }, CancellationToken.None);
    }

    #endregion // Methods
}