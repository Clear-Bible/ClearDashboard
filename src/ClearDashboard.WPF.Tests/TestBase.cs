using System;
using System.Diagnostics;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.Wpf.Application.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class TestBase
    {
        #nullable disable
        protected ITestOutputHelper Output { get; private set; }
        protected Process Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddClearDashboardDataAccessLayer();
            Services.AddMediatR(typeof(IMediatorRegistrationMarker));
            Services.AddLogging();
            Services.AddLocalization();
          
        }

    }
}
