using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear
{
    public class InterlinearDialogViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, InterlinearDialogViewModel>
    {
        #region Member Variables

        private TranslationSource? _translationSource;
        public ParallelCorpusId? ParallelCorpusId { get; set; }

        #endregion //Member Variables


        #region Observable Properties

        private AlignmentSetId? _selectedAlignmentSet;
        public AlignmentSetId? SelectedAlignmentSet
        {
            get => _selectedAlignmentSet;
            set => Set(ref _selectedAlignmentSet, value);
        }


        private List<AlignmentSetId>? _alignmentSets = new();
        public List<AlignmentSetId>? AlignmentSets
        {
            get => _alignmentSets;
            set => Set(ref _alignmentSets, value);
        }


        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }


        private bool _canCancel;
        public bool CanCancel
        {
            get => _canCancel;
            set => Set(ref _canCancel, value);
        }


        private string? _translationSetDisplayName;
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

        #endregion //Observable Properties


        #region Constructor

        public InterlinearDialogViewModel()
        {
            // no-op
        }

        public InterlinearDialogViewModel(ParallelCorpusId parallelCorpusId,
            TranslationSource? translationSource, INavigationService navigationService,
            ILogger<InterlinearDialogViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, IValidator<InterlinearDialogViewModel> validator, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            _translationSource = translationSource;
            ParallelCorpusId = parallelCorpusId;
            Logger!.LogInformation("'InterlinearDialogViewModel' ctor called.");
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

        protected override void OnViewLoaded(object view)
        {
            Console.WriteLine();

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        protected override ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(TranslationSetDisplayName)) ? Validator.Validate(this) : null;
        }

        public async void Create()
        {
            await TryCloseAsync(false);
        }

        #endregion // Methods
    }
}
