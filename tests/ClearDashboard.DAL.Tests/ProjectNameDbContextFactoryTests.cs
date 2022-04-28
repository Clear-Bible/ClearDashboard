using System.IO;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data;

namespace ClearDashboard.DAL.Tests
{
    public class ProjectNameDbContextFactoryTests: TestBase
    {
        public ProjectNameDbContextFactoryTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task TestProjectDatabaseCreation()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            const string projectName = "Project6";

            Assert.NotNull(factory);

            var context1 = await factory?.Get(projectName)!;

            Assert.NotNull(context1);
            var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
            var databaseName = $"{projectDirectory}\\{projectName}.sqlite";
            Assert.True(File.Exists(databaseName));


            // NB:  My 'Documents' folder is set with in my OneDrive folder 
            // which places a lock on the file so I'm not able to 
            // programmatically delete the database file.
            //File.Delete(databaseName);
            //Directory.Delete(projectDirectory, true);
        }
    }
}