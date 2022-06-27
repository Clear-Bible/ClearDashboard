using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Verse
{
    public class SetCurrentVerseCommandHandler : ParatextRequestHandler<SetCurrentVerseCommand, RequestResult<string>, string>
    {

        public SetCurrentVerseCommandHandler([NotNull] ILogger<SetCurrentVerseCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<string>> Handle(SetCurrentVerseCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("verse", request, cancellationToken, HttpVerb.PUT);
        }

    }
}
