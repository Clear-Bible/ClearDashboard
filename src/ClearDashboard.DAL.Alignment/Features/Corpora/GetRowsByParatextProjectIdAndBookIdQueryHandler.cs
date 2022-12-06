using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Microsoft.Extensions.Logging;

using ParatextRequest= ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm.GetRowsByParatextProjectIdAndBookIdQuery;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetRowsByParatextProjectIdAndBookIdQueryHandler : MediatorPassthroughRequestHandler<
        GetRowsByParatextProjectIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
    {

        public GetRowsByParatextProjectIdAndBookIdQueryHandler(IMediator mediator, ILogger<GetRowsByParatextProjectIdAndBookIdQueryHandler> logger) : base(mediator, logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>> Handle(GetRowsByParatextProjectIdAndBookIdQuery request, CancellationToken cancellationToken)
        {

            // TODO:  NEED TO FIX BY CALLING to PARATEXT!

            var paratextRequest = new ParatextRequest(request.ParatextProjectId, request.BookId);

            var result = await Mediator.Send(paratextRequest, CancellationToken.None).ConfigureAwait(false);
            
            if (result.Success == false)
            {
                return new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
                (result: new List<(string chapter, string verse, string text, bool isSentenceStart)>(),
                    success: false,
                    message: result.Message);
            }

            List<(string chapter, string verse, string text, bool isSentenceStart)> list = new();
            if (result.Data != null)
            {
                foreach (var data in result.Data)
                {
                    list.Add((data.Chapter, data.Verse, data.Text, data.isSentenceStart));
                }
            }

            return new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
                    (result: list, success: true, message: "");
        }
    }
}
