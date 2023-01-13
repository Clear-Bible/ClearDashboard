using System;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using ClearDashboard.DataAccessLayer.Threading;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;

public class AquaDoGetStepViewModel : DashboardApplicationWorkflowStepViewModel<IAquaGetCorpusAnalysisDialogViewModel>
{
    public AquaDoGetStepViewModel()
    {
    }
    public AquaDoGetStepViewModel(
        string paratextProjectId,
        string requestId,
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

        BodyTitle = "Do Get Body Title";
        BodyText = "Do Get Body Text";
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        return base.OnInitializeAsync(cancellationToken);
    }
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
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
        ParentViewModel?.Ok();
    }
    public void Cancel(object obj)
    {
        ParentViewModel?.Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }
    public async void Get()
    {
        try
        {
            var processStatus = await ParentViewModel!.GetAnalysis();

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
        catch (Exception)
        {
            ParentViewModel!.Cancel();
        }
    }
}