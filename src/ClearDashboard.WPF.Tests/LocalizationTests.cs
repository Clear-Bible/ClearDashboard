using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Helpers;
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
            var logger = ServiceProvider.GetService<ILogger<LocalizationTests>>();
            var text = LocalizationStrings.Get("Landing_NewProject", logger);
            Assert.Equal("New Project", text);


        }
    }
}
