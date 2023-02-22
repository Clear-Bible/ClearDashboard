using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;


namespace ClearDashboard.Wpf.Application.ViewModels.Popups
{
    public class ConfirmationDialogViewModel : WorkflowShellViewModel
    {
        public string ConfirmationText { get; set; }

        public ConfirmationDialogViewModel(INavigationService navigationService, ILogger<ConfirmationDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            DisplayName = "ClearDashboard";
        }

        public ConfirmationDialogViewModel() { }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
        }

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

    }
}
