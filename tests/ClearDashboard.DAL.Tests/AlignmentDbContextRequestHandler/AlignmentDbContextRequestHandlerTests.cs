using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
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

        [Fact]
        public async Task GetUsersViaHandlerTest()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var projectName = $"Alignment{random.Next(1, 1000)}";

            Output.WriteLine($"Creating database: {projectName}");
            SetupProjectDatabase(projectName, true);

            try
            {

                var user1 = new User { FirstName = "Bob", LastName = "Smith" };
                var user2 = new User { FirstName = "Janie", LastName = "Jones" };

                await ProjectDbContext.Users.AddRangeAsync(new[] { user1, user2 });
                await ProjectDbContext.SaveChangesAsync();

                var mediator = Container!.Resolve<IMediator>();

                var request = new GetUsersQuery("Dummy");
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
                await DeleteDatabaseContext(projectName);
            }
        }


    }
}
