
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.ProjectInfo;
using ClearDashboard.DAL.Tests.Slices.Users;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIL.Providers;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class DatabasePopulationSanityCheckTests : TestBase
    {
        #nullable disable
        public DatabasePopulationSanityCheckTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public Task CreateAlignmentDatabase()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Project{random.Next(1, 1000)}";

            Output.WriteLine($"Creating database: {projectName}");
            SetupProjectDatabase(projectName, false, false);
            Assert.NotNull(ProjectDbContext);

            Output.WriteLine($"Don't forget to delete the database: {projectName}.");
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ProjectInfoAddTest()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";

            Output.WriteLine($"Creating database: {projectName}");
            SetupProjectDatabase(projectName, false, false);

            try
            {
                var userProvider = Container!.Resolve<IUserProvider>();
                var testUser = new User { FirstName = "Test", LastName = "User" };
                userProvider.CurrentUser = testUser;

                ProjectDbContext.Users.Add(testUser);
                await ProjectDbContext.SaveChangesAsync();

                var projectInfo = new Project
                {
                    Id = Guid.NewGuid(),
                    IsRtl = true,
                    ProjectName = projectName
                };

                ProjectDbContext.Projects.Add(projectInfo);
                await ProjectDbContext.SaveChangesAsync();

                var roundTrippedProject = ProjectDbContext.Projects.FirstOrDefault(p => p.Id == projectInfo.Id);

                Assert.NotNull(roundTrippedProject);
                Assert.Equal(testUser.Id, roundTrippedProject.UserId);

                projectInfo.IsRtl = false;
                projectInfo.ProjectName = $"Updated {projectName}";

                //await context.AddCopyAsync(projectInfo);
                //await context.SaveChangesAsync();

                Assert.Equal(2, ProjectDbContext.Projects.Count());

                roundTrippedProject =
                    ProjectDbContext.Projects.OrderByDescending(project => project.Created).FirstOrDefault();

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
                await DeleteDatabaseContext(projectName);
            }
        }


        [Fact]
        public async Task ProjectInfoViaQueryAndCommandHandlersTest()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";

            Output.WriteLine($"Creating database: {projectName}");
            SetupProjectDatabase(projectName, false, false);

            try
            {
                var testUser = await AddDashboardUser(ProjectDbContext, true);
                var projectInfo = await AddCurrentProject(ProjectDbContext, projectName, true);

                // Create a copy of the project which is not attached
                // to the database context so we can compare it to 
                // an updated version later on.
                var copiedProject = Copy(projectInfo);

                var mediator = Container!.Resolve<IMediator>();

                // Save a project
                //var saveCommand = new AddProjectInfoCommand(projectName, new[] { projectInfo });
                //var savedResult = await mediator.Send(saveCommand);

                //Assert.NotNull(savedResult);
                //Assert.True(savedResult.Success);
                //Assert.True(savedResult.HasData);

                //var savedProject = savedResult.Data.FirstOrDefault();
                Assert.NotNull(projectInfo);
                Assert.Equal(testUser.Id, projectInfo.UserId);

                // Now get the project back
                var singleQuery = new GetProjectInfoQuery(projectInfo.Id);
                var singleResult = await mediator.Send(singleQuery);


                Assert.NotNull(singleResult);
                Assert.True(singleResult.Success);
                Assert.True(singleResult.HasData);

                Assert.Equal(projectInfo.Id, singleResult.Data.Id);
                Assert.Equal(projectInfo.ProjectName, singleResult.Data.ProjectName);
                Assert.Equal(projectInfo.IsRtl, singleResult.Data.IsRtl);

                var query = new GetProjectInfoListQuery();
                var result = await mediator.Send(query);

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.True(result.HasData);

                var roundTrippedProject = result.Data.FirstOrDefault();


                // Do some sanity checking
                Assert.NotNull(roundTrippedProject);
                Assert.Equal(testUser.Id, roundTrippedProject.UserId);

                Assert.Equal(projectInfo.Id, roundTrippedProject.Id);
                Assert.Equal(projectInfo.ProjectName, roundTrippedProject.ProjectName);
                Assert.Equal(projectInfo.IsRtl, roundTrippedProject.IsRtl);

                // Create a copy of the project which is not attached
                // to the database context so we can compare it to 
                // an updated version.
                copiedProject = Copy(projectInfo);

                // Now update the project
                copiedProject.IsRtl = false;
                copiedProject.ProjectName = $"Updated {projectName}";

                var updateCommand = new UpdateProjectInfoCommand(new[] { copiedProject });
                var updateResult = await mediator.Send(updateCommand);
                Assert.NotNull(updateResult);
                Assert.True(updateResult.Success);
                Assert.True(updateResult.HasData);

                var updatedProject = updateResult.Data.FirstOrDefault();

                Assert.NotEqual(projectInfo.ProjectName, updatedProject.ProjectName);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Output.WriteLine(message);
                throw;
            }
            finally
            {
                DeleteDatabaseContext(projectName);
            }
        }

        [Fact]
        public async Task CorpusMetadataTest()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";

            Output.WriteLine($"Creating database: {projectName}");
            SetupProjectDatabase(projectName, false, false);

            try
            {
                var corpus = new Corpus
                {
                    Name = "Test Corpus"
                };
                corpus.Metadata.Add("number", 1);
                corpus.Metadata.Add("string", "ha!");

                ProjectDbContext.Corpa.Add(corpus);
                await ProjectDbContext.SaveChangesAsync();

                var roundTrippedCorpus = await ProjectDbContext.Corpa.FirstOrDefaultAsync(c => c.Id == corpus.Id);

                Assert.NotNull(roundTrippedCorpus);
                Assert.True(roundTrippedCorpus.Metadata.Count == 2);


            }
            catch (Exception ex)
            {
                Output.WriteLine(ex.Message);
            }
            finally
            {
                await DeleteDatabaseContext(projectName);
            }
        }
    }
}
