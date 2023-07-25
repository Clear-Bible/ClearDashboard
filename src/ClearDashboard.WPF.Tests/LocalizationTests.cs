using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading;
using ClearApplicationFoundation.Services;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class LocalizationTests : TestBase
    {
        private readonly ILocalizationService? _localization;

        public LocalizationTests(ITestOutputHelper output) : base(output)
        {
            
            _localization = ServiceProvider.GetService<ILocalizationService>();
        }

        [Fact]

        public void GetLocalizedStringTest()
        {
            
            var text = _localization!.Get("Landing_NewProject");

            Assert.Equal("New Project", text);

            var indexerText = _localization["Landing_NewProject"];
            Assert.Equal("New Project", indexerText);
        }

        [Fact]
        public void GetRussianLocalizationStringTest()
        {
            var cultureInfo = new CultureInfo("ru");
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            var localization = ServiceProvider.GetService<ILocalizationService>();
            var text = localization!.Get("Landing_NewProject");

            Assert.Equal("Новый проект", text);

            var indexerText = localization["Landing_NewProject"];
            Assert.Equal("Новый проект", indexerText);

            cultureInfo = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }
        
    }
}
