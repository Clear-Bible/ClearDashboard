using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects;

public class GetProjectFontFamilyQueryHandler : ParatextRequestHandler<GetProjectFontFamilyQuery, RequestResult<string>, string>
{
    public GetProjectFontFamilyQueryHandler(ILogger<GetProjectFontFamilyQueryHandler> logger) : base(logger)
    {
    }

    public override async Task<RequestResult<string>> Handle(GetProjectFontFamilyQuery request, CancellationToken cancellationToken)
    {
        if (request.ParatextProjectId == ManuscriptIds.HebrewManuscriptId)
        {
            return new RequestResult<string>(FontNames.HebrewFontFamily);
        }

        if (request.ParatextProjectId == ManuscriptIds.GreekManuscriptId)
        {
            return new RequestResult<string>(FontNames.GreekFontFamily);
        }
        return await ExecuteRequest("project/fontfamily", request, cancellationToken);
    }
}