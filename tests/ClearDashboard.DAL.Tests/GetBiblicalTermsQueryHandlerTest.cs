﻿using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetBiblicalTermsQueryHandlerTest : TestBase
    {
        #nullable disable

        public GetBiblicalTermsQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetBiblicalTermRenderingTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var result =
                await ExecuteAndTestRequest<GetBiblicalTermsQuery, RequestResult<BiblicalTermsList>, BiblicalTermsList>(
                    new GetBiblicalTermsQuery(path, BTtype.BT));
        }

        [Fact]
        private async Task GetAllBiblicalTermRenderingTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var result =
                await ExecuteAndTestRequest<GetBiblicalTermsQuery, RequestResult<BiblicalTermsList>, BiblicalTermsList>(
                    new GetBiblicalTermsQuery(path, BTtype.allBT));

            Output.WriteLine($"Returned {result.Data.Term.Count} records.");
        }
    }
}
