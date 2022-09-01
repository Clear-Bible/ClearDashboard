using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    internal class ProjectSetupViewModel : WorkflowStepViewModel
    {

        public ProjectSetupViewModel(IEventAggregator eventAggregator, ILogger<MainViewModel> logger, IMediator mediator, INavigationService navigationService, ILifetimeScope lifetimeScope) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {

        }

        protected async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        public async Task Create()
        {
            var startupViewModel = Parent as StartupDialogViewModel;
            startupViewModel.Ok();
            //await TryCloseAsync(true);
        }
    }
}
