using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Notes
{
    public class AddNoteCommandHandler : ParatextRequestHandler<AddNoteCommand, RequestResult<ExternalNote>, ExternalNote>
    {

        public AddNoteCommandHandler([NotNull] ILogger<AddNoteCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<ExternalNote>> Handle(AddNoteCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("addnotecommand", request, cancellationToken);
        }
        
    }
}
