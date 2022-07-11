using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests;

public class GetCurrentParatextUserQueryHandlerTests : TestBase
{
    #nullable disable
    public GetCurrentParatextUserQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
        //no-op
    }

    [Fact]
    public async Task GetCurrentParatextUserTest()
    {
        var result = await ExecuteParatextAndTestRequest<GetCurrentParatextUserQuery, RequestResult<AssignedUser>, AssignedUser>(new GetCurrentParatextUserQuery());

        Assert.NotNull(result.Data.Name);
        Output.WriteLine($"User name: {result.Data.Name}");
    }


}