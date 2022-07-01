using System;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DAL.Tests.Mocks;
using ClearDashboard.DAL.Tests.Slices.Users;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests.AlignmentDbContextRequestHandler
{
    public class AlignmentDbContextRequestHandlerTests : TestBase
    {
        #nullable disable
        public AlignmentDbContextRequestHandlerTests(ITestOutputHelper output) : base(output)
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
        public async Task GetUsersViaHandlerTest()
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

                var user1 = new User { FirstName = "Bob", LastName = "Smith" };
                var user2 = new User { FirstName = "Janie", LastName = "Jones" };

                await context.Users.AddRangeAsync(new[] { user1, user2 });
                await context.SaveChangesAsync();

                var mediator = ServiceProvider.GetService<IMediator>();

                var request = new GetUsersQuery(projectName, "Dummy");

                var result = await mediator?.Send(request);

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.True(result.HasData);
                Assert.Equal(2, result.Data.Count);

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


    }
}
