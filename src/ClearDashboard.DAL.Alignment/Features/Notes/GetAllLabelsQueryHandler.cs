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
    public class GetAllLabelsQueryHandler : ProjectDbContextQueryHandler<
        GetAllLabelsQuery,
        RequestResult<IEnumerable<Label>>,
        IEnumerable<Label>>
    {
        private readonly IMediator _mediator;

        public GetAllLabelsQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllLabelsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Label>>> GetDataAsync(GetAllLabelsQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var labels = ProjectDbContext.Labels
                .Select(l => new Label(
                    new LabelId(l.Id),
                    l.Text ?? string.Empty
                    ));

            return new RequestResult<IEnumerable<Label>>
            (
                labels
            ); ;
        }
    }
}
