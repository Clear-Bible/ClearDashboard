using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Workflows;

public interface IWorkflowStepViewModel
{
    Direction Direction { get; set; }
    Task MoveForwards();
    Task MoveBackwards();
}

public abstract class WorkflowStepViewModel : ApplicationScreen, IWorkflowStepViewModel
{

    private Direction _direction;
    public Direction Direction
    {
        get => _direction;
        set => Set(ref _direction, value);
    }

    protected WorkflowStepViewModel()
    {

    }

    protected WorkflowStepViewModel(IEventAggregator eventAggregator, INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager) :
        base(navigationService, logger, projectManager, eventAggregator)
    {

    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ShowWorkflowButtons();
        EnableControls = (Parent as WorkflowShellViewModel).EnableControls;
        return base.OnActivateAsync(cancellationToken);
    }

    private bool enableControls_;
    public bool EnableControls
    {
        get => enableControls_;
        set
        {
            Logger.LogInformation($"WorkflowStepViewModel - Setting EnableControls to {value} at {DateTime.Now:HH:mm:ss.fff}");
            //(Parent as WorkflowShellViewModel).EnableControls = value;
            Set(ref enableControls_, value);
        }
    }


    public async Task MoveForwards()
    {
        Direction = Direction.Forwards;
        await TryCloseAsync();
    }

    public async Task MoveBackwards()
    {
        Direction = Direction.Backwards;
        await TryCloseAsync();
    }

    private bool _showBackButton;
    public bool ShowBackButton
    {
        get => _showBackButton;
        set => Set(ref _showBackButton, value);
    }


    private bool _showForwardButton;
    public bool ShowForwardButton
    {
        get => _showForwardButton;
        set => Set(ref _showForwardButton, value);
    }

    protected void ShowWorkflowButtons()
    {
        var steps = (Parent as WorkflowShellViewModel)?.Steps;
        if (steps != null)
        {
            var index = steps.IndexOf(this);

            if (index == 0 && steps.Count == 1)
            {

                ShowBackButton = false;
                ShowForwardButton = false;
            }

            if (index > 0 && index < steps.Count - 1)
            {

                ShowBackButton = true;
                ShowForwardButton = true;
            }


            if (index == 0 && steps.Count > 1)
            {

                ShowBackButton = false;
                ShowForwardButton = true;
            }

            if (steps.Count > 1 && index == steps.Count - 1)
            {
                ShowBackButton = true;
                ShowForwardButton = false;
            }


        }
    }
}