﻿using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests;

public class GetCurrentProjectQueryHandlerTests : TestBase
{
    #nullable disable
    public GetCurrentProjectQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
        //no-op
    }

    [Fact]
    public async Task GetCurrentProjectTest()
    {
        var result = await ExecuteParatextAndTestRequest<GetCurrentProjectQuery, RequestResult<ParatextProject>, ParatextProject>(new GetCurrentProjectQuery());

        Assert.NotNull(result.Data.ShortName);
        Output.WriteLine($"Project name: {result.Data.ShortName}");
    }

   
}