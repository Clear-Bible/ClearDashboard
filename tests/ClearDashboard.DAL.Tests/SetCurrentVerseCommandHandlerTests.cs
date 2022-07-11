using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class SetCurrentVerseCommandHandlerTests : TestBase
    {
        public SetCurrentVerseCommandHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        private async Task SetCurrentVerseTest()
        {
            var result =
                await ExecuteAndTestRequest<SetCurrentVerseCommand, RequestResult<string>, string>(
                    new SetCurrentVerseCommand("042001001"));

            Assert.True(result.Success);
        }
    }
}
