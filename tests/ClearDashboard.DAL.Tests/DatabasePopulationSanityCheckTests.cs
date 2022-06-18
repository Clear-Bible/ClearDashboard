
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
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

        [Fact]
        public async Task ProjectInfoAddTest()
        {
            var userProvider = ServiceProvider.GetService<IUserProvider>();
            Assert.NotNull(userProvider);

            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            var projectName = "Alignment";//Guid.NewGuid().ToString();
            Assert.NotNull(factory);

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

                await context.AddCopyAsync(projectInfo);
                await context.SaveChangesAsync();

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
                await context.Database.EnsureDeletedAsync();
                var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
                Directory.Delete(projectDirectory, true);
            }
        }
    }
}
