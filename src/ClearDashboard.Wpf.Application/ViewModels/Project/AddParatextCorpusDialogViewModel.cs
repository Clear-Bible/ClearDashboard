using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class AddParatextCorpusDialogViewModel : ValidatingApplicationScreen<AddParatextCorpusDialogViewModel>
    {
        private readonly DashboardProjectManager _projectManager;
        private CorpusSourceType _corpusSourceType;
        private List<ParatextProjectMetadata> _projects;
        private ParatextProjectMetadata _selectedProject;

        public AddParatextCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public AddParatextCorpusDialogViewModel(INavigationService? navigationService,
            ILogger<AddParatextCorpusDialogViewModel>? logger,
            DashboardProjectManager? projectManager,
        IEventAggregator? eventAggregator,
            IValidator<AddParatextCorpusDialogViewModel> validator, IMediator? mediator)
            : base(navigationService, logger, eventAggregator, mediator, validator)
        {
            _projectManager = projectManager;
        }

        public CorpusSourceType CorpusSourceType
        {
            get => _corpusSourceType;
            set => Set(ref _corpusSourceType, value);
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
            var result = await _projectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success)
            {
                Projects = result.Data.OrderBy(p => p.Name).ToList();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        protected override ValidationResult Validate()
        {
            return (SelectedProject != null) ? Validator.Validate(this) : null;
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
}
