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
    public class GetLabelByLabelIdQueryHandler : ProjectDbContextQueryHandler<
        GetLabelByLabelIdQuery,
        RequestResult<Label>,
        Label>
    {
        private readonly IMediator _mediator;

        public GetLabelByLabelIdQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLabelByLabelIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Label>> GetDataAsync(GetLabelByLabelIdQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var label = ProjectDbContext.Labels
                .FirstOrDefault(l => l.Id == request.LabelId.Id);
            if (label == null)
            {
                return new RequestResult<Label>
                (
                    success: false,
                    message: $"Label not found for LabelId '{request.LabelId.Id}'"
                );
            }

            return new RequestResult<Label>
            (
                new Label(
                    _mediator,
                    new LabelId(label.Id),
                    label.Text ?? string.Empty)
            ); ;
        }
    }
}
