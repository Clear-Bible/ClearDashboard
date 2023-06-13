using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class CreateOrUpdateLabelCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateLabelCommand,
        RequestResult<LabelId>, LabelId>
    {
        private readonly IMediator _mediator;

        public CreateOrUpdateLabelCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateOrUpdateLabelCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LabelId>> SaveDataAsync(CreateOrUpdateLabelCommand request,
            CancellationToken cancellationToken)
        {
            Models.Label? label = null;
            if (request.LabelId != null)
            {
                label = ProjectDbContext!.Labels.FirstOrDefault(c => c.Id == request.LabelId.Id);
                if (label == null)
                {
                    return new RequestResult<LabelId>
                    (
                        success: false,
                        message: $"Invalid LabelId '{request.LabelId.Id}' found in request"
                    );
                }

                label.Text = request.Text;
                label.TemplateText = request.TemplateText;
            }
            else
            {
                label = new Models.Label
                {
                    Text = request.Text,
                    TemplateText = request.TemplateText
                };

                ProjectDbContext.Labels.Add(label);
            }
            
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<LabelId>(new LabelId(label.Id));
        }
    }
}