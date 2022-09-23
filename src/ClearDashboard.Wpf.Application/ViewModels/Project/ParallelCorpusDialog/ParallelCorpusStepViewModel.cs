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
    
    public class ParallelCorpusStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<ParallelCorpusDialogViewModel, ParallelCorpus>
    {

       

        public ParallelCorpusStepViewModel()
        {

        }


        public ParallelCorpusStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParallelCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ParallelCorpus> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            CanMoveForwards = false;
            CanMoveBackwards = false;
            EnableControls = true;
            CanOk = true;
            CanCreate = true;

        }

     

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ParentViewModel.CurrentStepTitle =
                LocalizationStrings.Get("ParallelCorpusDialog_AddParallelRelationship", Logger);
            return base.OnActivateAsync(cancellationToken);
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
            try 
            {
                var status = await ParentViewModel?.AddParallelCorpus()!;

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
        }

        protected override ValidationResult? Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
