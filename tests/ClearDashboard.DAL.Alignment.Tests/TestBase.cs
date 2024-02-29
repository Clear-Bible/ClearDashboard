using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.CommandReceivers;
using ClearDashboard.DAL.Alignment.Commands;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Mocks;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ClearDashboard.DAL.Alignment.Tests
{
    public abstract class TestBase
    {
        public IMediator Mediator { get; private set; }
        public ProjectDbContext ProjectDbContext { get; set; }
        protected ITestOutputHelper Output { get; private set; }
        protected IContainer? Container { get; private set; }
        protected string ProjectName { get; set; }
        protected bool DeleteDatabase { get; set; } = true;
        public ILogger Logger { get; set; }

        private static Mutex mutex = new Mutex();

        protected TestBase(ITestOutputHelper output)
        {
            mutex.WaitOne();
            try
            {
                Output = output;
                // ReSharper disable once VirtualMemberCallInConstructor
                Container = SetupDependencyInjection();
                Mediator = Container!.Resolve<IMediator>();
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
                    RemotePersonalAccessToken = section["RemotePersonalAccessToken"],
                    RemotePersonalPassword = section["RemotePersonalPassword"]
                };
            });

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(configModule);
            builder.RegisterType<CollaborationManager>().AsSelf().SingleInstance();

			builder.RegisterType<GetVerseRangeTokensCommandReceiver>()
	            .As<IApiCommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<TokensTextRow> Rows, int IndexOfVerse)>>();

			// Register Paratext as our "External" lexicon provider / drafting tool:
			builder.RegisterType<ParatextPlugin.CQRS.Features.Lexicon.GetLexiconQuery>()
                .As<IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>>>()
                .Keyed<IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>>>("External");

            var container = builder.Build();
            return container;
        }

        protected virtual void AddServices(ServiceCollection services)
        {
            services.AddScoped<ProjectDbContext>();
            services.AddScoped<ProjectDbContextFactory>();
            services.AddScoped<DbContextOptionsBuilder<ProjectDbContext>, SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>();
            services.AddMediatR(
                typeof(CreateParallelCorpusCommandHandler), 
                typeof(ClearDashboard.DataAccessLayer.Features.Versification.GetVersificationAndBookIdByDalParatextProjectIdQueryHandler),
                typeof(MergeProjectSnapshotCommandHandler));
            services.AddLogging();
            services.AddSingleton<IUserProvider, UserProvider>();
            services.AddSingleton<IProjectProvider, ProjectProvider>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<TranslationCommands>();
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
