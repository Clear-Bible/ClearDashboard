using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using FluentValidation;
//using ClearDashboard.Wpf.Validators;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Enums;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{

    public class ParallelCorpusStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, ParallelCorpusStepViewModel>
    {

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties
        public ParallelProjectType ProjectType { get; set; }


        private string _parallelCorpusDisplayName;
        public string ParallelCorpusDisplayName
        {
            get => _parallelCorpusDisplayName;
            set
            {
                Set(ref _parallelCorpusDisplayName, value);
                ValidationResult = Validator.Validate(this);
                CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;

                ParentViewModel.CurrentProject = value;
            }
        }

        private DialogMode _dialogMode;
        public DialogMode DialogMode
        {
            get => _dialogMode;
            set => Set(ref _dialogMode, value);
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }




        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public ParallelCorpusStepViewModel()
        {

        }


        public ParallelCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParallelCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ParallelCorpusStepViewModel> validator, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            DialogMode = dialogMode;
            CanMoveForwards = false;
            CanMoveBackwards = false;
            EnableControls = true;
            CanOk = false;
            CanCreate = false;

        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ParentViewModel.CurrentStepTitle =
                LocalizationService!.Get("ParallelCorpusDialog_AddParallelRelationship");

            ParallelCorpusDisplayName =
                $"{ParentViewModel.SourceCorpusNodeViewModel.Name} - {ParentViewModel.TargetCorpusNodeViewModel.Name}";

            ProjectType = ParentViewModel.ProjectType;

            return base.OnActivateAsync(cancellationToken);
        }

        protected override async void OnViewReady(object view)
        {
            // skip this step if we are doing only creating a new alignment
            if (ParentViewModel.ProjectType == ParallelProjectType.AlignmentOnly)
            {
                await MoveForwards();
            }

            base.OnViewReady(view);

        }

        #endregion //Constructor


        #region Methods

        public void Ok()
        {
            ParentViewModel?.Ok();
        }


        public async void UseDefaults()
        {
            if (ParentViewModel is not null)
            {
                ParentViewModel.UseDefaults = true;
            }

            await Create(true);
        }


        public async void Create()
        {
            await Create(true);
        }


        public async Task Create(object nothing)
        {
            CanCreate = false;
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var status = await ParentViewModel?.AddParallelCorpus(ParallelCorpusDisplayName)!;

                    switch (status)
                    {
                        case LongRunningTaskStatus.Completed:
                            await MoveForwards();
                            break;
                        case LongRunningTaskStatus.Failed:
                        case LongRunningTaskStatus.Cancelled:
                            ParentViewModel.Cancel();
                            break;
                        case LongRunningTaskStatus.NotStarted:
                            break;
                        case LongRunningTaskStatus.Running:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    ParentViewModel!.Cancel();
                }
            }, CancellationToken.None);
        }

        protected override ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(ParallelCorpusDisplayName)) ? Validator.Validate(this) : null;
        }

        #endregion // Methods


    }
}
