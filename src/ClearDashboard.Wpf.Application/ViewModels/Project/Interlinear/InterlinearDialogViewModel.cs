using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear
{
    public class InterlinearDialogViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, InterlinearDialogViewModel>
    {

        private TranslationSource? _translationSource;
        private AlignmentSetId? _selectedAlignmentSet;

        public AlignmentSetId? SelectedAlignmentSet
        {
            get => _selectedAlignmentSet;
            set => Set(ref _selectedAlignmentSet, value);
        }

        private List<AlignmentSetId>? _alignmentSets;
        private bool _canCreate;
        private bool _canCancel;
        private string? _translationSetDisplayName;

        public List<AlignmentSetId>? AlignmentSets
        {
            get => _alignmentSets;
            set => Set(ref _alignmentSets, value);
        }

        public ParallelCorpusId? ParallelCorpusId { get; set; }

        public string TranslationSetDisplayName
        {
            get => _translationSetDisplayName;
            set
            {
                Set(ref _translationSetDisplayName, value);
                ValidationResult = Validator.Validate(this);
                CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
            }
        }

        public InterlinearDialogViewModel()
        {
            AlignmentSets = new List<AlignmentSetId>();
        }

        public InterlinearDialogViewModel(ParallelCorpusId parallelCorpusId, TranslationSource? translationSource, INavigationService navigationService,
            ILogger<InterlinearDialogViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, IValidator<InterlinearDialogViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            _translationSource = translationSource;
            ParallelCorpusId = parallelCorpusId;
            Logger!.LogInformation("'InterlinearDialogViewModel' ctor called.");
            AlignmentSets = new List<AlignmentSetId>();
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            AlignmentSets = (await AlignmentSet.GetAllAlignmentSetIds(
                Mediator!, 
                ParallelCorpusId,
                new UserId(ProjectManager!.CurrentUser.Id, ProjectManager.CurrentUser.FullName!)))
                .ToList();

            SelectedAlignmentSet = AlignmentSets.FirstOrDefault();
            CanCreate = false;
            CanCancel = true;
        }

        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        public bool CanCancel
        {
            get => _canCancel;
            set => Set(ref _canCancel, value);
        }

        public async void Create()
        {
            await TryCloseAsync(false);
        }

        protected override ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(TranslationSetDisplayName)) ? Validator.Validate(this) : null;
        }
    }
}
