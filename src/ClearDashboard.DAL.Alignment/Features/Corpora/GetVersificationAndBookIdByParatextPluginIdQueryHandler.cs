using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

//USE TO ACCESS Models

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetVersificationAndBookIdByParatextProjectIdQueryHandler : MediatorPassthroughRequestHandler<GetVersificationAndBookIdByParatextProjectIdQuery, RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>, (ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {
        private readonly IMediator _mediator;
        private ILogger<GetVersificationAndBookIdByParatextProjectIdQueryHandler> _logger;

        public GetVersificationAndBookIdByParatextProjectIdQueryHandler(IMediator mediator, ILogger<GetVersificationAndBookIdByParatextProjectIdQueryHandler> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        public override async Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>>
            Handle(GetVersificationAndBookIdByParatextProjectIdQuery request, CancellationToken cancellationToken)
        {
           var paratextRequest = new GetVersificationAndBookIdByDalParatextProjectIdQuery(request.ParatextProjectId);

            var result2 = await Mediator.Send(paratextRequest, cancellationToken);

          
            if (result2.Success == false)
            {
                return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (ScrVers.Original, new List<string>()), success: false, message: result2.Message);
            }

            return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (result2.Data.Versification, result2.Data.BookAbbreviations), success: true, message: "");
        }
    }
}
