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

        private ParallelCorpus _parallelCorprus;
        public ParallelCorpus ParallelCorpus
        {
            get => _parallelCorprus;
            private init => Set(ref _parallelCorprus, value);
        }

        public ParallelCorpusStepViewModel()
        {

        }


        public ParallelCorpusStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ParallelCorpusStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ParallelCorpus> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            ParallelCorpus = new ParallelCorpus();
           
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            ParentViewModel.CurrentStepTitle =
                LocalizationStrings.Get("ParallelCorpusDialog_AddParallelRelationship", Logger);
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override ValidationResult? Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
