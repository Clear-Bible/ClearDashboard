using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Mocks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.EntityFrameworkCore;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using Autofac.Configuration;
using System.Configuration;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.DAL.Alignment.Tests
{
    public abstract class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }
        protected IContainer? Container { get; private set; }

        protected IMediator? Mediator { get; private set; }

        protected ProjectDbContext? ProjectDbContext { get; set; }
        protected string? ProjectName { get; set; }
        protected bool DeleteDatabase { get; set; } = true;
        protected ILogger Logger { get; set; }

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
            SetupLogging(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\ClearDashboardTests.log"));
            SetupTests();
        }

        protected virtual void SetupDependencyInjection()
        {
            var services = new ServiceCollection();
            AddServices(services);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json");
            configBuilder.AddUserSecrets<TestBase>();

            var config = configBuilder.Build();
            var configModule = new ConfigurationModule(config);
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<CollaborationConfiguration>(sp =>
            {
                var c = sp.GetService<IConfiguration>();
                var section = c.GetSection("Collaboration");
                return new CollaborationConfiguration()
                {
                    RemoteUrl = section["RemoteUrl"],
                    RemoteEmail = section["RemoteEmail"],
                    RemoteUserName = section["RemoteUserName"],
                    RemotePassword = section["RemotePassword"]
                };
            });

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(configModule);
            builder.RegisterType<CollaborationManager>().AsSelf().SingleInstance();

            Container = builder.Build();
        }

        protected virtual void AddServices(ServiceCollection services)
        {
            services.AddScoped<ProjectDbContext>();
            services.AddScoped<ProjectDbContextFactory>();
            services.AddScoped<DbContextOptionsBuilder<ProjectDbContext>, SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>();
            services.AddMediatR(typeof(CreateParallelCorpusCommandHandler), typeof(ClearDashboard.DataAccessLayer.Features.Versification.GetVersificationAndBookIdByDalParatextProjectIdQueryHandler));
            services.AddLogging();
            services.AddSingleton<IUserProvider, UserProvider>();
            services.AddSingleton<IProjectProvider, ProjectProvider>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<TranslationCommands>();
        }

        private async void SetupTests()
        {
            Mediator = Container!.Resolve<IMediator>();

            var factory = Container!.Resolve<ProjectDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            ProjectName = $"Project{random.Next(1, 1000)}";
            Assert.NotNull(factory);

            Output.WriteLine($"Creating database: {ProjectName}");

            ProjectDbContext = await factory!.GetDatabaseContext(
                ProjectName,
                true).ConfigureAwait(false);

            _ = await AddDashboardUser(ProjectDbContext);
            _ = await AddCurrentProject(ProjectDbContext, ProjectName);
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
            var testProject = new Project { ProjectName = projectName, IsRtl = true };
            var projectProvider = Container!.Resolve<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = testProject;

            context.Projects.Add(testProject);
            await context.SaveChangesAsync();
            return testProject;
        }

        protected void SetupLogging(string logPath, LogEventLevel logLevel = LogEventLevel.Information, string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
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
            Logger = Container!.Resolve<ILogger<TestBase>>()!;
            Logger.LogDebug($"Test logging has been configured.  Writing logs to '{logPath}'");
        }
    }
}
