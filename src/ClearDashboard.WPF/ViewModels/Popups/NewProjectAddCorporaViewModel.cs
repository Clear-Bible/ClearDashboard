using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Popups;

public class NewProjectAddCorporaViewModel : ValidatingWorkflowStepViewModel<Project>
{
    private CorpusSourceType _corpusSourceType;

    public NewProjectAddCorporaViewModel()
    {
        // used by Caliburn Micro for design time    
    }

    public NewProjectAddCorporaViewModel(INavigationService navigationService,
        ILogger<WorkSpaceViewModel> logger,
        DashboardProjectManager projectManager,
        IEventAggregator eventAggregator,
        IValidator<Project> projectValidator)
        : base(eventAggregator, navigationService, logger, projectManager, projectValidator)
    {
    }

    public CorpusSourceType CorpusSourceType
    {
        get => _corpusSourceType;
        set => Set(ref _corpusSourceType,value);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        CorpusSourceType = CorpusSourceType.Paratext;
        CanMoveBackwards = true;
        return base.OnActivateAsync(cancellationToken);
    }

    protected override ValidationResult Validate()
    {
        throw new System.NotImplementedException();
    }
}