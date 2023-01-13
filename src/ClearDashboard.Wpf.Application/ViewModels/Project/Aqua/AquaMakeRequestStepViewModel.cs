using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;

public class AquaMakeRequestStepViewModel : DashboardApplicationWorkflowStepViewModel<IAquaRequestCorpusAnalysisDialogViewModel>
{
    public AquaMakeRequestStepViewModel()
    {
    }
    public AquaMakeRequestStepViewModel( 
        string paratextProjectId,

        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaMakeRequestStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        BodyTitle = "Make Request Body Title";
        BodyText = "Make Request Body Text";
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        return base.OnInitializeAsync(cancellationToken);
    }
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        return base.OnActivateAsync(cancellationToken);
    }


    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string? bodyTitle_;
    public string? BodyTitle
    {
        get => bodyTitle_;
        set
        {
            bodyTitle_ = value;
            NotifyOfPropertyChange(() => BodyTitle);
        }
    }

    private string? bodyText_;
    public string? BodyText
    {
        get => bodyText_;
        set
        {
            bodyText_ = value;
            NotifyOfPropertyChange(() => BodyText);
        }
    }


    public void Ok(object obj)
    {
        ((IAquaDialogViewModel)ParentViewModel!).Ok();
    }
    public void Cancel(object obj)
    {
        ((IAquaDialogViewModel)ParentViewModel!).Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }
    public async void Request()
    {
        try
        {
            var processStatus = await ParentViewModel!.RequestAnalysis();

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
                case LongRunningTaskStatus.Cancelled:
                    ((IAquaDialogViewModel)ParentViewModel!).Cancel();
                    break;
                case LongRunningTaskStatus.NotStarted:
                    break;
                case LongRunningTaskStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception)
        {
            ((IAquaDialogViewModel)ParentViewModel!).Cancel();
        }
    }
}