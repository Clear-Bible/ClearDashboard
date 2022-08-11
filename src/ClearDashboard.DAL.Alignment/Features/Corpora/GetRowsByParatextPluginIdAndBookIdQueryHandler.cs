using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetRowsByParatextProjectIdAndBookIdQueryHandler : ParatextRequestHandler<
        GetRowsByParatextProjectIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
    {

        public GetRowsByParatextProjectIdAndBookIdQueryHandler(ILogger<GetVersificationAndBookIdByParatextPluginIdQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>> Handle(GetRowsByParatextProjectIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            var result = await ExecuteRequest("bookusfmbyparatextidbookid", request, CancellationToken.None)
                .ConfigureAwait(false);

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
                    list.Add((data.chapter, data.verse, data.text, data.isSentenceStart));
                }
            }

            return new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
                    (result: list, success: true, message: "");
        }
    }
}
