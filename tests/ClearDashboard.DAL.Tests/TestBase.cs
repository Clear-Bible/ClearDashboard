using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }

        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected virtual void SetupDependencyInjection()
        {
           Services.AddClearDashboardDataAccessLayer();
           Services.AddLogging();
        }
    }
}
