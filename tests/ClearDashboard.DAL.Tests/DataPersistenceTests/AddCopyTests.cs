using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests.DataPersistenceTests
{
    public class AddCopyTests : TestBase
    {
        public AddCopyTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ObjectGraphTests()
        {
            var context = await GetTestContext("ObjectGraphTest.db");

            try
            {

            }
            finally
            {
                await context.Database.EnsureDeletedAsync();
            }
        }
    }
}
