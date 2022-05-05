using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Workflows;

public abstract class WorkflowStepViewModel : ApplicationScreen
{
    protected IEventAggregator EventAggregator { get; set; }
    protected ILogger Logger { get; set; }

    private Direction _direction;
    public Direction Direction
    {
        get => _direction;
        set => Set(ref _direction, value);
    }

    protected WorkflowStepViewModel()
    {
        
    }

    protected WorkflowStepViewModel(IEventAggregator eventAggregator, INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager) :base(navigationService, logger, projectManager, eventAggregator)
    {
        EventAggregator = eventAggregator;
        Logger = logger;
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
}