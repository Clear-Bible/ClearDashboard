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
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using System.IO;
using Xunit;
using System.Threading;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;

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
        public ProjectDbContext ProjectDbContext { get; set; }
        protected string ProjectName { get; set; }
        protected bool DeleteDatabase { get; set; } = true;
        protected ILogger Logger { get; set; }

        private static Mutex mutex = new Mutex();

        protected TestBase(ITestOutputHelper output)
        {
            mutex.WaitOne();
            try
            {
                Output = output;
                // ReSharper disable once VirtualMemberCallInConstructor
                Container = SetupDependencyInjection();
                Logger = SetupLogging(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\ClearDashboardTests.log"));
                (ProjectName, ProjectDbContext) = SetupTests().GetAwaiter().GetResult();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        protected virtual IContainer SetupDependencyInjection()
        {
            Services.AddScoped<ProjectDbContext>();
            Services.AddScoped<ProjectDbContextFactory>();
            Services.AddScoped<DbContextOptionsBuilder<ProjectDbContext>, SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>();
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddSingleton<IUserProvider, UserProvider>();
            Services.AddSingleton<IProjectProvider, ProjectProvider>();
			Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
	            typeof(IMediatorRegistrationMarker).Assembly,
	            typeof(CreateParallelCorpusCommandHandler).Assembly,
				typeof(ClearDashboard.DataAccessLayer.Features.Versification.GetVersificationAndBookIdByDalParatextProjectIdQueryHandler).Assembly,
				typeof(MergeProjectSnapshotCommandHandler).Assembly
            ));
            Services.AddLogging();
            ServiceCollectionExtensionsWpf.AddLocalization(Services);
            Services.AddSingleton<TranslationCommands>();
            Services.AddTransient<SelectedBookManager>();

            var builder = new ContainerBuilder();
            builder.Populate(Services);
            builder.RegisterType<CollaborationManager>().AsSelf().SingleInstance();
            builder.RegisterType<LongRunningTaskManager>().AsSelf().SingleInstance();
            builder.RegisterType<SystemPowerModes>().AsSelf().SingleInstance();
            builder.RegisterType<ProjectTemplateProcessRunner>().AsSelf().SingleInstance();

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

        private async Task<(string, ProjectDbContext)> SetupTests()
        {
            var factory = Container!.Resolve<ProjectDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Project{random.Next(1, 10000)}";
            Assert.NotNull(factory);

            Output.WriteLine($"Creating database: {projectName}");

            var projectDbContext = await factory!.GetDatabaseContext(
                projectName,
                true).ConfigureAwait(false);

            _ = await AddDashboardUser(projectDbContext);
            _ = await AddCurrentProject(projectDbContext, projectName);

            return (projectName, projectDbContext);
        }

        protected async Task DeleteDatabaseContext()
        {
            if (DeleteDatabase)
            {
                Output.WriteLine($"Deleting database: {ProjectName}");
                await ProjectDbContext!.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{ProjectName}";
                Directory.Delete(projectDirectory, true);

                Container!.Dispose();
            }
        }

        protected async Task<User> AddDashboardUser(ProjectDbContext context)
        {
            var testUser = new User { FirstName = "Test", LastName = "User" };
            var userProvider = Container!.Resolve<IUserProvider>();
            Assert.NotNull(userProvider);
            userProvider!.CurrentUser = testUser;

            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            return testUser;
        }

        protected async Task<Project> AddCurrentProject(ProjectDbContext context, string projectName)
        {
            var testProject = new Project { ProjectName = projectName, IsRtl = true, AppVersion = "9.9.9.9" };
            var projectProvider = Container!.Resolve<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = testProject;

            context.Projects.Add(testProject);
            await context.SaveChangesAsync();
            return testProject;
        }

        protected ILogger SetupLogging(string logPath, LogEventLevel logLevel = LogEventLevel.Information, string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
        {
#if DEBUG
            logLevel = LogEventLevel.Verbose;
#endif
            var log = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .WriteTo.File(logPath, outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug(outputTemplate: outputTemplate)
                .CreateLogger();
            var loggerFactory = Container!.Resolve<ILoggerFactory>();
            loggerFactory.AddSerilog(log);
            var logger = Container!.Resolve<ILogger<TestBase>>()!;
            logger.LogDebug($"Test logging has been configured.  Writing logs to '{logPath}'");

            return logger;
        }
    }
}
