using System;
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
            Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetLanguageResourcesQuery).Assembly));
            Services.AddLogging();
        }

        [Fact]
        public async Task GetLanguagesTest()
        {
            var mediator = ServiceProvider.GetService<IMediator>();

            if (mediator == null)
            {
                throw new NullReferenceException("IMediator has not been set up in DI wire up!");
            }
            var result = await mediator.Send(new GetLanguageResourcesQuery());

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Success);
            Assert.NotEmpty(result.Data!);

            Assert.Contains("English", result.Data!);
            Assert.Contains("Russian", result.Data!);

            Output.WriteLine("Languages in file:");
            foreach (var language in result.Data!)
            {
                Output.WriteLine(language);
            }

        }
    }
}
