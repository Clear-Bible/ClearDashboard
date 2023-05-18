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
    public class CreateLabelGroupAssociationCommandHandler : ProjectDbContextCommandHandler<CreateLabelGroupAssociationCommand,
        RequestResult<LabelGroupAssociationId>, LabelGroupAssociationId>
    {
        private readonly IMediator _mediator;

        public CreateLabelGroupAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateLabelGroupAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LabelGroupAssociationId>> SaveDataAsync(CreateLabelGroupAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var labelGroupAssociation = ProjectDbContext!.LabelGroupAssociations
                .FirstOrDefault(ln => ln.LabelId == request.LabelId.Id && ln.LabelGroupId == request.LabelGroupId.Id);

            if (labelGroupAssociation == null)
            {
                labelGroupAssociation = new Models.LabelGroupAssociation
                {
                    LabelId = request.LabelId.Id,
                    LabelGroupId = request.LabelGroupId.Id
                };

                ProjectDbContext.LabelGroupAssociations.Add(labelGroupAssociation);
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<LabelGroupAssociationId>(new LabelGroupAssociationId(labelGroupAssociation.Id));
        }
    }
}