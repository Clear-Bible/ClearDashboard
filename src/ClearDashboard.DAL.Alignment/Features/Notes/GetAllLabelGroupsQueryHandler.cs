using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class GetAllLabelGroupsQueryHandler : ProjectDbContextQueryHandler<
        GetAllLabelGroupsQuery,
        RequestResult<IEnumerable<LabelGroup>>,
        IEnumerable<LabelGroup>>
    {
        private readonly IMediator _mediator;

        public GetAllLabelGroupsQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllLabelGroupsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<LabelGroup>>> GetDataAsync(GetAllLabelGroupsQuery request, CancellationToken cancellationToken)
        {
            var labelGroups = ProjectDbContext.LabelGroups
                .Select(l => new LabelGroup(
                    new LabelGroupId(l.Id),
                    l.Name
                ));

            return new RequestResult<IEnumerable<LabelGroup>>
            (
                await labelGroups.ToListAsync(cancellationToken: cancellationToken)
            );
        }
    }
}
