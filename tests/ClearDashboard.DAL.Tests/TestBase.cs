using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.LanguageResources;
using ClearDashboard.DAL.Tests.Slices.Users;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Interceptors;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Models;
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
        #nullable disable
        protected ITestOutputHelper Output { get; private set; }
        protected Process Process { get; set; }
        protected bool StopParatextOnTestConclusion { get; set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected IMediator? Mediator => ServiceProvider.GetService<IMediator>();
        protected ProjectDbContext? ProjectDbContext { get; set; }
        protected string? ProjectName { get; set; }

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
            SetupTests();
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddSingleton<IWindowManager, WindowManager>();
            Services.AddSingleton<INavigationService>(sp=> null);
            Services.AddClearDashboardDataAccessLayer();
            Services.AddMediatR(typeof(IMediatorRegistrationMarker), typeof(CreateParallelCorpusCommandHandler));
            Services.AddLogging();
            Services.AddSingleton<IUserProvider, UserProvider>();
            Services.AddSingleton<IProjectProvider, ProjectProvider>();
            Services.AddScoped<ProjectDbContext>();
            Services.AddScoped<ProjectDbContextFactory>();
            Services.AddScoped<SqliteDatabaseConnectionInterceptor>();
        }
        private async void SetupTests()
        {
            var factory = ServiceProvider.GetService<ProjectDbContextFactory>();
            ProjectName = $"EnhancedView";
            Assert.NotNull(factory);

            Output.WriteLine($"Opening database: {ProjectName}");
            var assets = await factory?.Get(ProjectName)!;
            ProjectDbContext = assets.ProjectDbContext;
            
            var testUser = await AddDashboardUser(ProjectDbContext);
            SetupProjectProvider(ProjectDbContext);
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
            var mediator = ServiceProvider.GetService<IMediator>()!;

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

        protected async Task<User> AddDashboardUser(ProjectDbContext context)
        {
            var testUser = new User { FirstName = "Test", LastName = "User" };
            var userProvider = ServiceProvider.GetService<IUserProvider>();
            Assert.NotNull(userProvider);
            userProvider!.CurrentUser = testUser;

            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            return testUser;
        }

        protected void SetupProjectProvider(ProjectDbContext context)
        {
            var projectProvider = ServiceProvider.GetService<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = context.Projects.First();
        }

        protected async Task<Project> AddCurrentProject(ProjectDbContext context, string projectName)
        {
            var testProject = new Project { ProjectName = projectName, IsRtl = true };
            var projectProvider = ServiceProvider.GetService<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = testProject;

            context.Projects.Add(testProject);
            await context.SaveChangesAsync();
            return testProject;
        }
    }
}
