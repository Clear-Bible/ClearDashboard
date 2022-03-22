using System.IO;
using ClearDashboard.DataAccessLayer.Context;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Tests
{
    public class ProjectNameDbContextFactoryTests: TestBase
    {
        public ProjectNameDbContextFactoryTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task TestMultipleProjects()
        {
            var factory = ServiceProvider.GetService<ProjectNameDbContextFactory>();
            const string projectName = "Project5";

            Assert.NotNull(factory);

            var context1 = await factory.Create(projectName);

            Assert.NotNull(context1);
            var projectDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\CLEAR_Projects\\{projectName}";
            var databaseName = $"{projectDirectory}\\{projectName}.sqlite";
            Assert.True(File.Exists(databaseName));


            //Allow the database files to be create.
            //await Task.Delay(TimeSpan.FromSeconds(30));

            //File.Delete(databaseName);
            //Directory.Delete(projectDirectory, true);
        }
    }
}