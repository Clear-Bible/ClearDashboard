using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.ViewModels.Workflows;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Popups;

public class NewProjectAddCorporaViewModel : ValidatingWorkflowStepViewModel<DataAccessLayer.Models.Project>
{
    private CorpusSourceType _corpusSourceType;
    private List<ParatextProjectMetadata> _projects;
    private ParatextProjectMetadata _selectedProject;

    public NewProjectAddCorporaViewModel()
    {
        // used by Caliburn Micro for design time    
    }

    public NewProjectAddCorporaViewModel(INavigationService navigationService,
        ILogger<WorkSpaceViewModel> logger,
        DashboardProjectManager projectManager,
        IEventAggregator eventAggregator,
        IValidator<DataAccessLayer.Models.Project> projectValidator)
        : base(eventAggregator, navigationService, logger, projectManager, projectValidator)
    {
    }

    public CorpusSourceType CorpusSourceType
    {
        get => _corpusSourceType;
        set => Set(ref _corpusSourceType,value);
    }

    public List<ParatextProjectMetadata> Projects
    {
        get => _projects;
        set => Set(ref _projects, value);
    }

    public ParatextProjectMetadata SelectedProject
    {
        get => _selectedProject;
        set => Set(ref _selectedProject, value);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        CorpusSourceType = CorpusSourceType.Paratext;
        CanMoveBackwards = true;
        var result = await ProjectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
        if (result.Success)
        {
            Projects = result.Data.OrderBy(p=>p.Name).ToList();
        }

        await base.OnActivateAsync(cancellationToken);
    }

    protected override ValidationResult Validate()
    {
        throw new System.NotImplementedException();
    }
}