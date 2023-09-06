using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DataAccessLayer.Models.Common;
using GetLexiconQuery = ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon.GetLexiconQuery;

namespace ClearDashboard.DAL.Tests
{
    public class GetLexiconModelQueryHandlerTest : TestBase
    {
        #nullable disable
        public GetLexiconModelQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        public async Task GetLexiconModelQueryTest()
        {
            // get the HEB/GRK source language project type
            var result =
                await ExecuteParatextAndTestRequest<GetLexiconQuery, RequestResult<Lexicon_Lexicon>, Lexicon_Lexicon>(
                    new GetLexiconQuery());

            Assert.True(result.HasData);
            Assert.NotNull(result.Data);
        }
    }
}
