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
            var projectName = Guid.NewGuid().ToString();
            SetupProjectDatabase(projectName, true);

            try
            {
                Assert.NotNull(ProjectDbContext);
                var databaseName = $"{GetProjectDirectory(projectName)}\\{projectName}.sqlite";
                Assert.True(File.Exists(databaseName));
            }
            finally
            {
                await DeleteDatabaseContext(projectName);
            }
        }
    }
}