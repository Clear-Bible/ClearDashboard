using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features.BookUsfm
{

    public class BookUsfmByParatextIdBookIdController : FeatureSliceController
    {
        public BookUsfmByParatextIdBookIdController(IMediator mediator, ILogger<BookUsfmByParatextIdBookIdController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<UsfmVerse>>> GetAsync([FromBody] GetBookUsfmByParatextIdBookIdQuery command)
        {
            var result =
                await ExecuteRequestAsync<RequestResult<List<UsfmVerse>>,
                    List<UsfmVerse>>(command, CancellationToken.None);
            return result;

        }

    }
}
