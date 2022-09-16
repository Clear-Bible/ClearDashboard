using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Exceptions;
using System.Linq;
using ClearApplicationFoundation.Extensions;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class StartupDialogViewModel : WorkflowShellViewModel, IStartupDialog
    {
        public StartupDialogViewModel(INavigationService navigationService, ILogger<StartupDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = "Startup Dialog";
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("Startup", "Order").ToArray();
            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency inject registrations of 'IWorkflowStepViewModel' with the key of 'Startup'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = (Steps.Count == 1);

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
