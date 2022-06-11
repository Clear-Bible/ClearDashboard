using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests;

public class GetBiblicalTermsByTypeQueryHandlerTests : TestBase
{
    public GetBiblicalTermsByTypeQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
        //no-op
    }

    [Fact]
    public async Task GetAllBiblicalTermsTest()
    {
       var result =  await ExecuteParatextAndTestRequest<GetBiblicalTermsByTypeQuery, RequestResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>(new GetBiblicalTermsByTypeQuery(BiblicalTermsType.All));
    }

    [Fact]
    public async Task GetProjectBiblicalTermsTest()
    {
        var result = await ExecuteParatextAndTestRequest<GetBiblicalTermsByTypeQuery, RequestResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>(new GetBiblicalTermsByTypeQuery(BiblicalTermsType.Project));
    }
}