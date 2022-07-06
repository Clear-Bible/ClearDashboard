using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class SetCurrentVerseCommandHandlerTests : TestBase
    {
        public SetCurrentVerseCommandHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SetCurrentVerseTestAsync()
        {

            string verse = "040001001";
            try
            {
                await StartParatextAsync();

                var client = CreateHttpClient();

                var response =
                    await client.PutAsJsonAsync<SetCurrentVerseCommand>("verse", new SetCurrentVerseCommand(verse));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<string>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);
                Assert.Equal(result.Data, verse);

                Output.WriteLine(result.Data);
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}
