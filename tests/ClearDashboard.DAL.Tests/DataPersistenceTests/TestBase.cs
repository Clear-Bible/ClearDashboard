using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Tests.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests.DataPersistenceTests
{
    public abstract class TestBase
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
            Services.AddScoped<TestContext>();
            Services.AddLogging();
        }

        protected async Task<TestContext> GetTestContext(string fullPath)
        {
            var context = ServiceProvider.GetService<TestContext>();
            if (context != null)
            {
                try
                {
                
                    context.DatabasePath = fullPath;
                    await context.Migrate();
                }
                catch (Exception?)
                {
                    throw;
                }
                return context;
            }
            throw new NullReferenceException("Please ensure 'TestContext' has been registered with the dependency injection container.");
        }
    }
}
