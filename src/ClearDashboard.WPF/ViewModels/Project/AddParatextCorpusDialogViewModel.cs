using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using static ClearDashboard.Wpf.ViewModels.Project.ProjectDesignSurfaceViewModel;

namespace ClearDashboard.Wpf.ViewModels.Project;

public class AddParatextCorpusDialogViewModel : ValidatingApplicationScreen<AddParatextCorpusDialogViewModel>
{
    private CorpusSourceType _corpusSourceType;
    private List<ParatextProjectMetadata> _projects;
    private ParatextProjectMetadata _selectedProject;

    public AddParatextCorpusDialogViewModel()
    {
        // used by Caliburn Micro for design time    
    }

    public AddParatextCorpusDialogViewModel(INavigationService navigationService,
        ILogger<WorkSpaceViewModel> logger,
        DashboardProjectManager projectManager,
        IEventAggregator eventAggregator,
        IValidator<AddParatextCorpusDialogViewModel> validator)
        : base(navigationService, logger, projectManager, eventAggregator, validator)
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

    private Tokenizer _selectedTokenizer = Tokenizer.LatinWordTokenizer;
    public Tokenizer SelectedTokenizer
    {
        get => _selectedTokenizer;
        set => Set(ref _selectedTokenizer, value);
    }

    public ParatextProjectMetadata SelectedProject
    {
        get => _selectedProject;
        set
        {
            Set(ref _selectedProject, value);
            ValidationResult = Validator.Validate(this);
            CanOk = ValidationResult.IsValid;
        }
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        CorpusSourceType = CorpusSourceType.Paratext;
        var result = await ProjectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
        if (result.Success)
        {
            Projects = result.Data.OrderBy(p=>p.Name).ToList();
        }

        await base.OnActivateAsync(cancellationToken);
    }

    protected override ValidationResult Validate()
    {
        return (SelectedProject!=null) ? Validator.Validate(this) : null;
    }

    private bool _canOk;
    public bool CanOk
    {
        get => _canOk;
        set => Set(ref _canOk, value);
    }

    public async void Ok()
    {
        await TryCloseAsync(true);
    }

    public bool CanCancel => true /* can always cancel */;
    public async void Cancel()
    {
       await TryCloseAsync(false);
    }
}