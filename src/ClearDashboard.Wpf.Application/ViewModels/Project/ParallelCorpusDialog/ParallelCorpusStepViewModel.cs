using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
//using ClearDashboard.Wpf.Validators;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

using ParallelCorpus = ClearDashboard.DataAccessLayer.Models.ParallelCorpus;
namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{

    public class ParallelCorpusStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, ParallelCorpusStepViewModel>
    {



        public ParallelCorpusStepViewModel()
        {

        }


        public ParallelCorpusStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParallelCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ParallelCorpusStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            DialogMode = dialogMode;
            CanMoveForwards = false;
            CanMoveBackwards = false;
            EnableControls = true;
            CanOk = false;
            CanCreate = false;

        }

        private string _parallelCorpusDisplayName;
        public string ParallelCorpusDisplayName
        {
            get => _parallelCorpusDisplayName;
            set
            {
                Set(ref _parallelCorpusDisplayName, value);
                ValidationResult = Validator.Validate(this);
                CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
            }
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ParentViewModel.CurrentStepTitle =
                LocalizationStrings.Get("ParallelCorpusDialog_AddParallelRelationship", Logger);

            ParallelCorpusDisplayName =
                $"{ParentViewModel.SourceCorpusNodeViewModel.Name} - {ParentViewModel.TargetCorpusNodeViewModel.Name}";
            return base.OnActivateAsync(cancellationToken);
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

        public void Ok()
        {
            ParentViewModel?.Ok();
        }


        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        public async void Create()
        {
            CanCreate = false;
            ParentViewModel!.CreateCancellationTokenSource();
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var status = await ParentViewModel?.AddParallelCorpus(ParallelCorpusDisplayName)!;

                    switch (status)
                    {
                        case ProcessStatus.Completed:
                            await MoveForwards();
                            break;
                        case ProcessStatus.Failed:
                            ParentViewModel.Cancel();
                            break;
                        case ProcessStatus.NotStarted:
                            break;
                        case ProcessStatus.Running:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    ParentViewModel!.Cancel();
                }
            }, ParentViewModel!.CancellationTokenSource!.Token);
        }

        protected override ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(ParallelCorpusDisplayName)) ? Validator.Validate(this) : null;
        }
    }
}
