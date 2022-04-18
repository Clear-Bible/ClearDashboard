using System.Threading.Tasks;
using ClearDashboard.DAL.Tests.Slices.LanguageResources;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class XmlResourceHandlerTests : TestBase
    {
        public XmlResourceHandlerTests(ITestOutputHelper output) : base(output)
        {

        }

        protected override void SetupDependencyInjection()
        {
            Services.AddMediatR(typeof(GetLanguageResourcesCommand));
            Services.AddLogging();
            //base.SetupDependencyInjection();
        }

        [Fact]
        public async Task GetLanguagesTest()
        {
            var mediator = ServiceProvider.GetService<IMediator>();
            var result = await mediator.Send(new GetLanguageResourcesCommand());

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotEmpty(result.Data);

            Assert.Contains("English", result.Data);
            Assert.Contains("Russian", result.Data);

            Output.WriteLine("Languages in file:");
            foreach (var language in result.Data)
            {
                Output.WriteLine(language);
            }

        }
    }
}
