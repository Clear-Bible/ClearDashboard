using System;

using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.Wpf.Application.ViewModels.Main
{
    public  class MainViewModel : DashboardApplicationScreen
    {
        public MainViewModel()
        {

        }

        public MainViewModel(DashboardProjectManager projectManager, INavigationService? navigationService, ILogger<MainViewModel>? logger, IEventAggregator? eventAggregator, IMediator mediator, IServiceProvider serviceProvider) : base(projectManager, navigationService, logger, eventAggregator, mediator)
        {

            var projectDbContextFactory = serviceProvider.GetService<ProjectDbContextFactory>();

            projectDbContextFactory.Get("TestDependendcyInjection");
        }
    }
}
