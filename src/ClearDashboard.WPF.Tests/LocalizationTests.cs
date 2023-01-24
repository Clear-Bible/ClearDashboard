using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class LocalizationTests : TestBase
    {
        public LocalizationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]

        public void GetLocalizedStringTest()
        {
            var localization = ServiceProvider.GetService<ILocalizationService>();
            var text = localization.Get("Landing_NewProject");

            Assert.Equal("New Project", text);

            var indexerText = localization["Landing_NewProject"];
            Assert.Equal("New Project", indexerText);
        }
    }
}
