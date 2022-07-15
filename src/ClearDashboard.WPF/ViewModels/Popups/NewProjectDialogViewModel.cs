using System;
using System.Drawing.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Popups;

public class NewProjectDialogViewModel : WorkflowShellViewModel
{
    private NewProjectViewModel _newProjectViewModel;

    public string ProjectName => _newProjectViewModel.ProjectName;

    private bool _canCreate;

    public NewProjectDialogViewModel(DashboardProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator) 
        : base(projectManager, serviceProvider, logger, navigationService, eventAggregator)
    {
    }
    

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);

        _newProjectViewModel = ServiceProvider.GetService<NewProjectViewModel>();


        Steps.Add(_newProjectViewModel);

        var step2 = ServiceProvider.GetService<NewProjectAddCorporaViewModel>();
        Steps.Add(step2);

        CurrentStep = Steps[0];

        IsLastWorkflowStep = (Steps.Count == 1);
        await ActivateItemAsync(Steps[0], cancellationToken);
    }

    public bool CanCancel => true /* can always cancel */;

    public async void Cancel()
    {
        await TryCloseAsync(false);
    }

    public bool CanCreate
    {
        get => _canCreate;
        set => Set(ref _canCreate, value);
    }

    public async void Create()
    {
        await TryCloseAsync(true);
    }
}