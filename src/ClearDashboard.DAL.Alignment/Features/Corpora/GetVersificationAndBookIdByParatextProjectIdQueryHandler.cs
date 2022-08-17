using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

using ParatextRequest = ClearDashboard.ParatextPlugin.CQRS.Features.Versification.GetVersificationAndBookIdByParatextProjectIdQuery;

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
           var paratextRequest = new ParatextRequest(request.ParatextProjectId);

            var result = await Mediator.Send(paratextRequest, cancellationToken);

          
            if (result.Success == false)
            {
                return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (ScrVers.Original, new List<string>()), success: false, message: result.Message);
            }

            return new RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>
                (result: (result.Data.Versification, result.Data.BookAbbreviations), success: true, message: "");
        }
    }
}
