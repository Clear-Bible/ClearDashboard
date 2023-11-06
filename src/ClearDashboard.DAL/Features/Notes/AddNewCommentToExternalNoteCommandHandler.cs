using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.BiblicalTerms
{
    public class AddNewCommentToExternalNoteCommandHandler : ParatextRequestHandler<AddNewCommentToExternalNoteCommand, RequestResult<string>, string>
    {

        public AddNewCommentToExternalNoteCommandHandler([NotNull] ILogger<AddNewCommentToExternalNoteCommand> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<string>> Handle(AddNewCommentToExternalNoteCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("addnewcommenttoexternalnotecommand", request, cancellationToken);
        }
        
    }
}
