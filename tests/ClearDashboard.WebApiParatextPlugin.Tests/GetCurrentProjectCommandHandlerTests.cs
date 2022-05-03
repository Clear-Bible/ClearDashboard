
using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.Data.Features.Project;
using ClearDashboard.ParatextPlugin.Data.Models;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetCurrentProjectCommandHandlerTests : TestBase
    {

       

        public GetCurrentProjectCommandHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override void SetupDependencyInjection()
        {
           base.SetupDependencyInjection();
        }

        [Fact]
        public async Task TestCommand()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:9000/api/");

            var response = await client.PostAsJsonAsync<GetCurrentProjectCommand>("project", new GetCurrentProjectCommand());

            Assert.True(response.IsSuccessStatusCode);
            var result = await response.Content.ReadAsAsync<QueryResult<Project>>();
                    
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        
            
        }
    }
}
