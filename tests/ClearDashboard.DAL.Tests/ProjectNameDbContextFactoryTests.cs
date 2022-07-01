using System.IO;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data;
using SIL.Providers;

namespace ClearDashboard.DAL.Tests
{
    public class ProjectNameDbContextFactoryTests : TestBase
    {
        #nullable disable
        public ProjectNameDbContextFactoryTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task TestProjectDatabaseCreation()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            var projectName = Guid.NewGuid().ToString();
            var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";

            Assert.NotNull(factory);

            var assets = await factory?.Get(projectName)!;
            var context = assets.AlignmentContext;

            try
            {
                Assert.NotNull(assets);
                var databaseName = $"{projectDirectory}\\{projectName}.sqlite";
                Assert.True(File.Exists(databaseName));
            }
            finally
            {
                await context.Database.EnsureDeletedAsync();
                Directory.Delete(projectDirectory, true);
            }
        }
    }
}