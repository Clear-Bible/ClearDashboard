using Autofac;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.ProjectInfo;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class TestBase
    {
        #nullable disable
        protected ITestOutputHelper Output { get; private set; }
        protected Process Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
        protected IContainer? Container { get; private set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected IMediator? Mediator { get; private set; }
        protected ProjectDbContext? ProjectDbContext { get; set; }
        protected string? ProjectName { get; set; }

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddSingleton<IWindowManager, WindowManager>();
            Services.AddSingleton<INavigationService>(sp=> null);
            Services.AddClearDashboardDataAccessLayer();
            Services.AddScoped<ProjectDbContext>();
            Services.AddScoped<ProjectDbContextFactory>();
            Services.AddScoped<DbContextOptionsBuilder<ProjectDbContext>, SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>();
            Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(IMediatorRegistrationMarker).Assembly, 
                typeof(GetProjectInfoQueryHandler).Assembly
            ));
            //Services.AddMediatR(typeof(CreateParallelCorpusCommandHandler), typeof(ClearDashboard.DataAccessLayer.Features.Versification.GetVersificationAndBookIdByDalParatextProjectIdQueryHandler));
            Services.AddLogging();
            Services.AddSingleton<IUserProvider, UserProvider>();
            Services.AddSingleton<IProjectProvider, ProjectProvider>();

            var builder = new ContainerBuilder();

            // Mediator resolves this from the container, and generally 
            // as a thick client app, there isn't any notion of 'requests',
            // so most likely this will be resolved on the 'root' scope:
            builder.RegisterType<ProjectDbContextFactory>().InstancePerLifetimeScope();

            // Intended to be resolved/disposed at a 'request' level:
            builder.RegisterType<ProjectDbContext>().InstancePerRequest();
            builder.RegisterType<SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>().As<DbContextOptionsBuilder<ProjectDbContext>>().InstancePerRequest();

            builder.Populate(Services);

            Container = builder.Build();
            Mediator = Container!.Resolve<IMediator>();
        }
 
        protected async void SetupProjectDatabase(string projectName, bool leaveDbEmpty, bool userProjectAlreadyInDb = false)
        {
            var factory = Container!.Resolve<ProjectDbContextFactory>();
            Assert.NotNull(factory);

            Output.WriteLine($"Opening database: {projectName}");

            ProjectDbContext = await factory!.GetDatabaseContext(
                projectName,
                true).ConfigureAwait(false);

            if (!leaveDbEmpty)
            {
                _ = await AddDashboardUser(ProjectDbContext, userProjectAlreadyInDb);
                _ = await AddCurrentProject(ProjectDbContext, projectName, userProjectAlreadyInDb);
            }
        }

        protected static string GetProjectDirectory(string projectName)
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
        }

        protected async Task DeleteDatabaseContext(string projectName)
        {
            Output.WriteLine($"Deleting database: {projectName}");
            await ProjectDbContext!.Database.EnsureDeletedAsync();
            Directory.Delete(GetProjectDirectory(projectName), true);

            Container!.Dispose();
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
            var mediator = Container!.Resolve<IMediator>()!;

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
                Process?.Kill(true);

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

        protected T Copy<T>(T entity)
        {
            var json = JsonSerializer.Serialize(entity);
            return JsonSerializer.Deserialize<T>(json);
        }

        protected async Task<User> AddDashboardUser(ProjectDbContext context, bool alreadyInDb)
        {
            User? user = alreadyInDb ? context.Users.FirstOrDefault() : null;
            if (user is null)
            {
                user = new User { FirstName = "Test", LastName = "User" };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var userProvider = Container!.Resolve<IUserProvider>();
            Assert.NotNull(userProvider);
            userProvider!.CurrentUser = user;

            return user;
        }

        protected async Task<Project> AddCurrentProject(ProjectDbContext context, string projectName, bool alreadyInDb)
        {
            Project? project = alreadyInDb ? context.Projects.FirstOrDefault() : null;
            if (project is null)
            {
                project = new Project { ProjectName = projectName, IsRtl = true };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
            }

            var projectProvider = Container!.Resolve<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = project;

            return project;
        }
    }
}
