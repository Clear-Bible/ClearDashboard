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
     public class GetLabelsByPartialTextQueryHandler : ProjectDbContextQueryHandler<GetLabelsByPartialTextQuery,
        RequestResult<IEnumerable<Label>>, IEnumerable<Label>>
    {
        private readonly IMediator _mediator;

        public GetLabelsByPartialTextQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLabelsByPartialTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Label>>> GetDataAsync(GetLabelsByPartialTextQuery request, CancellationToken cancellationToken)
        {
            var labelsQueryable = ProjectDbContext.Labels
                .Where(l => l.Text != null);

            if (request.PartialText is not null)
            {
                labelsQueryable = labelsQueryable.Where(l => l.Text!.StartsWith(request.PartialText));
            }

            if (request.LabelGroupId is not null)
            {
                labelsQueryable = labelsQueryable.Where(l => l.LabelGroups.Any(lg => lg.Id == request.LabelGroupId.Id));
            }

            var labels = await labelsQueryable
                .Select(l => new Label(
                    new LabelId(l.Id),
                    l.Text ?? string.Empty,
                    l.TemplateText
                ))
                .ToListAsync(cancellationToken: cancellationToken);

            return new RequestResult<IEnumerable<Label>>(labels);
        }
    }
}
