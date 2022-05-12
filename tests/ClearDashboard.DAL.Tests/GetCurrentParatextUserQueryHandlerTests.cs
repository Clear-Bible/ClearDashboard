using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests;

public class GetCurrentParatextUserQueryHandlerTests : TestBase
{
    public GetCurrentParatextUserQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
        //no-op
    }

    [Fact]
    public async Task GetCurrentParatextUserTest()
    {
        var result = await ExecuteAndTestRequest<GetCurrentParatextUserQuery, RequestResult<AssignedUser>, AssignedUser>(new GetCurrentParatextUserQuery());

        Assert.NotNull(result.Data.Name);
        Output.WriteLine($"User name: {result.Data.Name}");
    }


}