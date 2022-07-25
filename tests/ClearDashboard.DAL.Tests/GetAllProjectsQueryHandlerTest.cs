using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetAllProjectsQueryHandlerTest : TestBase
    {
#nullable disable
        public GetAllProjectsQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
            //no-op
        }


        [Fact]
        public async Task GetAllProjects()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetAllProjectsQuery, RequestResult<List<ParatextProject>>,
                    List<ParatextProject>>(new GetAllProjectsQuery());
            Assert.NotEmpty(results.Data);
            Assert.True(results.HasData);
            Assert.True(results.Success);

            foreach (var project in results.Data)
            {
                Output.WriteLine($"{project.Name} :: {project.LongName}");
                Output.WriteLine($"   ID: {project.Guid}");
                Output.WriteLine($"{project.Name},  {project.Type}");
            }
        }

        [Fact]
        public async Task GetParatextProjectMetadata()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetProjectMetadataQuery, RequestResult<List<ParatextProjectMetadata>>,
                    List<ParatextProjectMetadata>>(new GetProjectMetadataQuery());


            foreach (var project in results.Data)
            {
                Output.WriteLine($"Name: {project.Name}, LongName: {project.LongName} \t CorpusType: {project.CorpusType}, \t LongName: {project.LanguageName}, ");
            }
        }
    }
}
