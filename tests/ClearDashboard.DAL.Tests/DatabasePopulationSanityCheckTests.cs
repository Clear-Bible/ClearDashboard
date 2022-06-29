
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.ProjectInfo;
using ClearDashboard.DAL.Tests.Slices.Users;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SIL.Providers;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class DatabasePopulationSanityCheckTests : TestBase
    {
        public DatabasePopulationSanityCheckTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override void SetupDependencyInjection()
        {
            Services.AddSingleton<IEventAggregator, EventAggregator>();
            Services.AddClearDashboardDataAccessLayer();
            Services.AddMediatR(typeof(GetUsersQueryHandler));
            Services.AddLogging();
            Services.AddSingleton<IUserProvider, UserProvider>();
        }

        [Fact]
        public async Task CreateAlignmentDatabase()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";
            Assert.NotNull(factory);

            Output.WriteLine($"Creating database: {projectName}");
            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            Output.WriteLine($"Don't forget to delete the database: {projectName}.");
        }

        [Fact]
        public async Task ProjectInfoAddTest()
        {
            var userProvider = ServiceProvider.GetService<IUserProvider>();
            Assert.NotNull(userProvider);

            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";
            Assert.NotNull(factory);
            Output.WriteLine($"Creating database: {projectName}");
            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            try
            {
                var testUser = new User { FirstName = "Test", LastName = "User" };
                userProvider.CurrentUser = testUser;

                context.Users.Add(testUser);
                await context.SaveChangesAsync();

                var projectInfo = new ProjectInfo
                {
                    IsRtl = true,
                    ProjectName = projectName
                };

                context.ProjectInfos.Add(projectInfo);
                await context.SaveChangesAsync();

                var roundTrippedProject = context.ProjectInfos.FirstOrDefault();

                Assert.NotNull(roundTrippedProject);
                Assert.Equal(testUser.Id, roundTrippedProject.UserId);

                projectInfo.IsRtl = false;
                projectInfo.ProjectName = $"Updated {projectName}";

                //await context.AddCopyAsync(projectInfo);
                //await context.SaveChangesAsync();

                Assert.Equal(2, context.ProjectInfos.Count());

                roundTrippedProject =
                    context.ProjectInfos.OrderByDescending(project => project.Created).FirstOrDefault();

                Assert.NotNull(roundTrippedProject);
                Assert.Equal(testUser.Id, roundTrippedProject.UserId);
                Assert.NotEqual(roundTrippedProject.Id, projectInfo.Id);


            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            finally
            {
                Output.WriteLine($"Deleting database: {projectName}");
                await context.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
                Directory.Delete(projectDirectory, true);
            }
        }


        [Fact]
        public async Task ProjectInfoViaQueryAndCommandHandlersTest()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";
            Assert.NotNull(factory);

            Output.WriteLine($"Creating database: {projectName}");
            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            try
            {
                var testUser = await AddDashboardUser(context);

                var projectInfo = new ProjectInfo
                {
                    IsRtl = true,
                    ProjectName = projectName
                };

                // Create a copy of the project which is not attached
                // to the database context so we can compare it to 
                // an updated version later on.
                var copiedProject = Copy(projectInfo);

                var mediator = ServiceProvider.GetService<IMediator>();

                // Save a project
                var saveCommand = new AddProjectInfoCommand(projectName, new[] { projectInfo });
                var savedResult = await mediator.Send(saveCommand);

                Assert.NotNull(savedResult);
                Assert.True(savedResult.Success);
                Assert.True(savedResult.HasData);

                var savedProject = savedResult.Data.FirstOrDefault();
                Assert.NotNull(savedProject);
                Assert.Equal(testUser.Id, savedProject.UserId);

                // Now get the project back
                var singleQuery = new GetProjectInfoQuery(projectName, savedProject.Id);
                var singleResult = await mediator.Send(singleQuery);
                

                Assert.NotNull(singleResult);
                Assert.True(singleResult.Success);
                Assert.True(singleResult.HasData);

                Assert.Equal(savedProject, singleResult.Data);

                var query = new GetProjectInfoListQuery(projectName);
                var result = await mediator.Send(query);

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.True(result.HasData);

                var roundTrippedProject = result.Data.FirstOrDefault();


                // Do some sanity checking
                Assert.NotNull(roundTrippedProject);
                Assert.Equal(testUser.Id, roundTrippedProject.UserId);

                Assert.Equal(savedProject, roundTrippedProject);


                // Create a copy of the project which is not attached
                // to the database context so we can compare it to 
                // an updated version.
                copiedProject = Copy(projectInfo);

                // Now update the project
                projectInfo.IsRtl = false;
                projectInfo.ProjectName = $"Updated {projectName}";

                var updateCommand = new UpdateProjectInfoCommand(projectName, new[] { projectInfo });
                var updateResult = await mediator.Send(updateCommand);
                Assert.NotNull(updateResult);
                Assert.True(updateResult.Success);
                Assert.True(updateResult.HasData);

                var updatedProject = updateResult.Data.FirstOrDefault();

                Assert.NotEqual(savedProject, copiedProject);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Output.WriteLine(message);
                throw;
            }
            finally
            {
                Output.WriteLine($"Deleting database: {projectName}");
                await context.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
                Directory.Delete(projectDirectory, true);
            }
        }
    }
}
