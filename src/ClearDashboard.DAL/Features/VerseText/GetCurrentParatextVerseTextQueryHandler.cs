using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.VerseText
{
    public class GetParatextVerseTextQueryHandler : ParatextRequestHandler<GetParatextVerseTextQuery, RequestResult<AssignedUser>, AssignedUser>
    {
        public GetParatextVerseTextQueryHandler(ILogger<GetParatextVerseTextQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<AssignedUser>> Handle(GetParatextVerseTextQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("versetext", request, cancellationToken);
        }
    }
}
