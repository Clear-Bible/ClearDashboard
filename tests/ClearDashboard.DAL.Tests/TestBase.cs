using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.LanguageResources;
using ClearDashboard.DAL.Tests.Slices.Users;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }
        protected Process? Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
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
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddClearDashboardDataAccessLayer();
            Services.AddMediatR(typeof(IMediatorRegistrationMarker));
            Services.AddLogging();
            Services.AddSingleton<IUserProvider, UserProvider>();
        }

        protected async Task<RequestResult<TData>> ExecuteParatextAndTestRequest<TRequest, TResult, TData>(
            TRequest query)
            where TRequest : IRequest<RequestResult<TData>>
            where TResult : RequestResult<TData>, new()
            where TData : class, new()
        {
            try
            {
                await StartParatext();
                return await ExecuteAndTestRequest<TRequest, TResult, TData>(query);
            }
            finally
            {
                await StopParatext();
            }
        }

        protected async Task<RequestResult<TData>> ExecuteAndTestRequest<TRequest, TResult, TData>(TRequest query)
            where TRequest : IRequest<RequestResult<TData>>
            where TResult : RequestResult<TData>, new()
        {
            var mediator = ServiceProvider.GetService<IMediator>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = new RequestResult<TData>(default(TData), false);
            try
            {
                result = await mediator.Send(query);
            }
            finally
            {
                stopwatch.Stop();
            }

            Assert.NotNull(result);
            Assert.True(result.Success);

            var type = result.Data.GetType();
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(ObservableCollection<>)))
            {
                Assert.NotEmpty((IEnumerable)result.Data);
                Output.WriteLine($"Returned {((IEnumerable<object>)(result.Data)).Count()} records in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
            else
            {
                Output.WriteLine($"Returned {type.Name} in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }

            return result;

        }

        protected async Task StartParatext()
        {
            var paratext = Process.GetProcessesByName("Paratext");

            if (paratext.Length == 0)
            {
                Output.WriteLine("Starting Paratext.");
                Process = await InternalStartParatext();
                StopParatextOnTestConclusion = true;

                var seconds = 2;
                Output.WriteLine($"Waiting for {seconds} seconds for Paratext to complete initialization.");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                Process = paratext[0];
                Output.WriteLine("Paratext is already running.");
            }


        }

        protected async Task StopParatext()
        {
            if (StopParatextOnTestConclusion)
            {
                Output.WriteLine("Stopping Paratext.");
                Process.Kill(true);

                Process = null;

                var seconds = 2;
                Output.WriteLine($"Waiting for {seconds} seconds for Paratext to stop.");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
        }

        private async Task<Process> InternalStartParatext()
        {
            var paratextInstallDirectory = Environment.GetEnvironmentVariable("ParatextInstallDir");
            var process = Process.Start($"{paratextInstallDirectory}\\paratext.exe");

            return process;
        }
    }
}
