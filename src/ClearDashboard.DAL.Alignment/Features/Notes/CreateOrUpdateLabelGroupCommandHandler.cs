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
    public class CreateOrUpdateLabelGroupCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateLabelGroupCommand,
        RequestResult<LabelGroupId>, LabelGroupId>
    {
        private readonly IMediator _mediator;

        public CreateOrUpdateLabelGroupCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateOrUpdateLabelGroupCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LabelGroupId>> SaveDataAsync(CreateOrUpdateLabelGroupCommand request,
            CancellationToken cancellationToken)
        {
            Models.LabelGroup? labelGroup = null;
            if (request.LabelGroupId != null)
            {
                labelGroup = ProjectDbContext!.LabelGroups.FirstOrDefault(c => c.Id == request.LabelGroupId.Id);
                if (labelGroup == null)
                {
                    return new RequestResult<LabelGroupId>
                    (
                        success: false,
                        message: $"Invalid LabelGroupId '{request.LabelGroupId.Id}' found in request"
                    );
                }

                labelGroup.Name = request.Name;
            }
            else
            {
                labelGroup = new Models.LabelGroup
                {
                    Name = request.Name
                };

                ProjectDbContext.LabelGroups.Add(labelGroup);
            }
            
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<LabelGroupId>(new LabelGroupId(labelGroup.Id));
        }
    }
}