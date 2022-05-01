using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Tests.Slices.LanguageResources;
using ClearDashboard.DataAccessLayer.Features.ManuscriptVerses;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetManuscriptVerseByIdHandlerTests : TestBase
    {
        public GetManuscriptVerseByIdHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override void SetupDependencyInjection()
        {
            Services.AddMediatR(typeof(GetManuscriptVerseByIdHandler));
            Services.AddLogging();
        }

        [Fact]
        public async Task InvokeGetManuscriptVerseByIdHandlerTest()
        {
            var mediator = ServiceProvider.GetService<IMediator>();

            var results = await mediator.Send(new GetManuscriptVerseByIdQuery("40005015"));

            Assert.NotNull(results);
            Assert.NotEmpty(results.Data);
            Assert.True(results.Data.Count == 48);
        }
    }
}
