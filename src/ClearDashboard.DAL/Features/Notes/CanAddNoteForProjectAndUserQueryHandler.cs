using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Notes
{
    public class CanAddNoteForProjectAndUserQueryHandler : ParatextRequestHandler<CanAddNoteForProjectAndUserQuery, RequestResult<bool>, bool>
    {

        public CanAddNoteForProjectAndUserQueryHandler([NotNull] ILogger<AddNoteCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<bool>> Handle(CanAddNoteForProjectAndUserQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("canaddnoteforprojectanduserquery", request, cancellationToken);
        }
    }
}
