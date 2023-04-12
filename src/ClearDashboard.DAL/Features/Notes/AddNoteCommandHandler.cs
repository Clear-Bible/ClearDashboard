using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;

namespace ClearDashboard.DataAccessLayer.Features.Notes
{
    public class AddNoteCommandHandler : ParatextRequestHandler<AddNoteCommand, RequestResult<IProjectNote>, IProjectNote>
    {

        public AddNoteCommandHandler([NotNull] ILogger<AddNoteCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IProjectNote>> Handle(AddNoteCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("addnotecommand", request, cancellationToken);
        }
        
    }
}
