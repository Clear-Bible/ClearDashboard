using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Validators;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class ParatextCorpusStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, ParatextCorpusStepViewModel>
    {
        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

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

        #endregion //Public Properties


        #region Observable Properties

        private string _paratextCorpusDisplayName;
        public string ParatextCorpusDisplayName
        {
            get => _paratextCorpusDisplayName;
            set
            {
                Set(ref _paratextCorpusDisplayName, value);
                //ValidationResult = Validator.Validate(this);
                //CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;

                ParentViewModel.CurrentProject = value;
            }
        }


        #endregion //Observable Properties


        #region Constructor

        public ParatextCorpusStepViewModel()
        {
            // no-op
        }

        public ParatextCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParatextCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, 
            IValidator<ParatextCorpusStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
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
                LocalizationStrings.Get("ParallelCorpusDialog_AddParallelRelationship", Logger);

            ParatextCorpusDisplayName = $"{ParentViewModel.CurrentProject}";
            return base.OnActivateAsync(cancellationToken);
        }


        #endregion //Constructor


        #region Methods

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
                    var status = await ParentViewModel?.AddParatextCorpus(ParatextCorpusDisplayName)!;

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
            return (!string.IsNullOrEmpty(ParatextCorpusDisplayName)) ? Validator.Validate(this) : null;
        }


        #endregion // Methods



    }
}
