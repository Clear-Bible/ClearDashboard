

using System.Threading.Tasks;
using Autofac;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class ProjectDesignSurfaceDataTests : TestBase
    {
#nullable disable
        public ProjectDesignSurfaceDataTests(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public async Task GetDataTest()
        {
            var contextFactory = Container.Resolve<ProjectDbContextFactory>();
            var dbContext = await contextFactory.GetDatabaseContext("c");

            Assert.NotNull(dbContext);

            var tokenizedCorpora = dbContext.TokenizedCorpora.Include(tc => tc.Corpus)
                .Include(tc => tc.SourceParallelCorpora).Include(tc => tc.TargetParallelCorpora);


            var json = JsonConvert.SerializeObject(tokenizedCorpora, Formatting.Indented, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

            Output.WriteLine(json);

        }

    }
}
