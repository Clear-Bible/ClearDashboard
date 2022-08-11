using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using Microsoft.Extensions.Logging;
using SIL.Scripture;


//USE TO ACCESS Models

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : ParatextRequestHandler<
        GetVersificationAndBookIdByParatextProjectIdQuery,
        RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>,
        (ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {

        public GetVersificationAndBookIdByParatextPluginIdQueryHandler(ILogger<GetVersificationAndBookIdByParatextPluginIdQueryHandler> logger):base(logger)
        {
        }
        
        public override async Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
            Handle(GetVersificationAndBookIdByParatextProjectIdQuery request, CancellationToken cancellationToken)
        {
            var result = await ExecuteRequest("versificationbooksbyparatextid", request, CancellationToken.None)
                .ConfigureAwait(false);

            if (result.Success == false)
            {
                return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (ScrVers.Original, new List<string>()), success: false, message: result.Message);
            }

            return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (result.Data.versification, result.Data.bookAbbreviations), success: true, message: "");
        }
    }
}
