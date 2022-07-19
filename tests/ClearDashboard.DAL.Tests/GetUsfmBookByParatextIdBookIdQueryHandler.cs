using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.DAL.Tests
{
    public class GetUsfmBookByParatextIdBookIdQueryHandler : TestBase
    {
        public GetUsfmBookByParatextIdBookIdQueryHandler(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetUsfmBookByParatextIdBookIdTest()
        {
            try
            {
                await StartParatext();

                var result =
                    new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>(
                        default(IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>), false);
                try
                {
                    var mediator = ServiceProvider.GetService<IMediator>()!;

                    //result = await mediator.Send(GetBookUsfmByParatextIdBookIdQuery);
                }
                finally
                {
                    // no-op
                }
            }
            finally
            {
                await StopParatext();
            }



            //result =
            //    await ExecuteParatextAndTestRequest<GetBookUsfmByParatextIdBookIdQuery,
            //        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>, IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>(
            //        new GetBookUsfmByParatextIdBookIdQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff", 1));
        }

    }
}
