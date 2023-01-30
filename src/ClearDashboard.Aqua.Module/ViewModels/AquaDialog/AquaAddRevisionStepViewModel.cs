using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;


namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaAddRevisionStepViewModel : DashboardApplicationWorkflowStepViewModel<IAquaDialogViewModel>
{
    public AquaAddRevisionStepViewModel()
    {
    }
    public AquaAddRevisionStepViewModel(
        IAquaManager aquaManager,

        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaAddRevisionStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope,
        ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        BodyTitle = "Add Revision Body Title";
        BodyText = "Add Revision Body Text";
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
        ParentViewModel!.Ok();
    }
    public void Cancel(object obj)
    {
        ParentViewModel!.Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }
    public async void AddRevision()
    {
        try
        {
            var processStatus = await ParentViewModel!.AddRevision();

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
                case LongRunningTaskStatus.Cancelled:
                    ParentViewModel!.Cancel();
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