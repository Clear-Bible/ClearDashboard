using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Notes
{
    public class ResolveExternalNoteCommandHandler : ParatextRequestHandler<ResolveExternalNoteCommand, RequestResult<string>, string>
    {

        public ResolveExternalNoteCommandHandler([NotNull] ILogger<ResolveExternalNoteCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<string>> Handle(ResolveExternalNoteCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("resolveexternalnotecommand", request, cancellationToken);
        }
        
    }
}
