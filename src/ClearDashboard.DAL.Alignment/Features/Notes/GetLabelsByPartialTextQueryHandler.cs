using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
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
            var labels = ProjectDbContext.Labels
                .Where(l => l.Text != null && l.Text.StartsWith(request.PartialText))
                .Select(l => new Label(
                    new LabelId(l.Id),
                    l.Text!
                    ));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Label>>(labels.ToList());
        }
    }
}
