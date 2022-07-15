using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Mocks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests
{
    public abstract class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }
        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected IMediator? Mediator => ServiceProvider.GetService<IMediator>();

        protected ProjectDbContext? ProjectDbContext { get; set; }
        protected string? ProjectName { get; set; }
        protected bool DeleteDatabase { get; set; } = true;

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
            SetupTests();
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddScoped<ProjectDbContext>();
            Services.AddScoped<ProjectDbContextFactory>();
            Services.AddMediatR(typeof(CreateParallelCorpusCommandHandler));
            Services.AddLogging();
            Services.AddSingleton<IUserProvider, UserProvider>();
            Services.AddSingleton<IProjectProvider, ProjectProvider>();
        }

        private async void SetupTests()
        {
            var factory = ServiceProvider.GetService<ProjectDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            ProjectName = $"Project{random.Next(1, 1000)}";
            Assert.NotNull(factory);

            Output.WriteLine($"Creating database: {ProjectName}");
            var assets = await factory?.Get(ProjectName)!;
            ProjectDbContext= assets.ProjectDbContext;

            var testUser = await AddDashboardUser(ProjectDbContext);
            var projectInfo = await AddCurrentProject(ProjectDbContext, ProjectName);
        }

        protected async Task DeleteDatabaseContext()
        {
            if (DeleteDatabase)
            {
                Output.WriteLine($"Deleting database: {ProjectName}");
                await ProjectDbContext!.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{ProjectName}";
                Directory.Delete(projectDirectory, true);
            }
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

        protected async Task<ProjectInfo> AddCurrentProject(ProjectDbContext context, string projectName)
        {
            var testProject = new ProjectInfo { ProjectName = projectName, IsRtl = true };
            var projectProvider = ServiceProvider.GetService<IProjectProvider>();
            Assert.NotNull(projectProvider);
            projectProvider!.CurrentProject = testProject;

            context.ProjectInfos.Add(testProject);
            await context.SaveChangesAsync();
            return testProject;
        }
    }
}
