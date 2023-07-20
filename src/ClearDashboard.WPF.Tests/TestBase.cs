using Autofac;
using Autofac.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.WPF.Tests.Mocks;

namespace ClearDashboard.WPF.Tests
{
    public class TestBase
    {
        #nullable disable
        protected ITestOutputHelper Output { get; private set; }
        protected Process Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        protected IContainer? Container { get; private set; }
        private IServiceProvider _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            Container = SetupDependencyInjection();
        }

        protected virtual IContainer SetupDependencyInjection()
        {
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddSingleton<IUserProvider, UserProvider>();
            Services.AddSingleton<IProjectProvider, ProjectProvider>();
            Services.AddMediatR(typeof(IMediatorRegistrationMarker));
            Services.AddLogging();
            Services.AddLocalization();
            Services.AddTransient<SelectedBookManager>();
            Services.AddScoped<ProjectDbContext>();
            Services.AddScoped<ProjectDbContextFactory>();
            Services.AddScoped<DbContextOptionsBuilder<ProjectDbContext>, SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>();

            var builder = new ContainerBuilder();
            builder.Populate(Services);
            builder.RegisterType<CollaborationManager>().AsSelf().SingleInstance();

            return builder.Build();
        }

        protected async Task StartParatextAsync()
        {
            var paratext = Process.GetProcessesByName("Paratext");

            if (paratext.Length == 0)
            {
                Output.WriteLine("Starting Paratext.");
                Process = await InternalStartParatextAsync();
                StopParatextOnTestConclusion = true;

                var seconds = 10;
                Output.WriteLine($"Waiting for {seconds} seconds for Paratext to complete initialization.");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                Process = paratext[0];
                Output.WriteLine("Paratext is already running.");
            }


        }

        protected async Task StopParatextAsync()
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

        private async Task<Process> InternalStartParatextAsync()
        {
            var paratextInstallDirectory = Environment.GetEnvironmentVariable("ParatextInstallDir");
            var process = Process.Start($"{paratextInstallDirectory}\\paratext.exe");

            return process;
        }
    }
}
