using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{
    public class ParallelCorpusDialogViewModel : WorkflowShellViewModel
    {

        public ParallelCorpusDialogViewModel(INavigationService navigationService, ILogger<ParallelCorpusDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = "Parallel Corpus Dialog";
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ParallelCorpusDialog", "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency inject registrations of 'IWorkflowStepViewModel' with the key of 'ParallelCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = Steps.Count == 1;

            EnableControls = true;
            await ActivateItemAsync(Steps[0], cancellationToken);

            await base.OnInitializeAsync(cancellationToken);
        }


        public bool CanCancel => true /* can always cancel */;
        public async void Cancel()
        {
            await TryCloseAsync(false);
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

        public object? ExtraData { get; set; }
    }
}
