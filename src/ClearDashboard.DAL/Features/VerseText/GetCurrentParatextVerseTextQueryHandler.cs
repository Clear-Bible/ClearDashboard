using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.VerseText
{
    public class GetCurrentParatextVerseTextQueryHandler : ParatextRequestHandler<GetCurrentParatextVerseTextQuery, RequestResult<AssignedUser>, AssignedUser>
    {
        public GetCurrentParatextVerseTextQueryHandler(ILogger<GetCurrentParatextVerseTextQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<AssignedUser>> Handle(GetCurrentParatextVerseTextQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("versetext", request, cancellationToken);
        }
    }
}
