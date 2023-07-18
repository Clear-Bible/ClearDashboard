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
    public class DeleteLabelGroupAssociationCommandHandler : ProjectDbContextCommandHandler<DeleteLabelGroupAssociationCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteLabelGroupAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteLabelGroupAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteLabelGroupAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var labelGroupAssociation = ProjectDbContext!.LabelGroupAssociations.FirstOrDefault(ln => 
                ln.LabelId == request.LabelId.Id && ln.LabelGroupId == request.LabelGroupId.Id);
            if (labelGroupAssociation == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid LabelId '{request.LabelId}' / LabelGroupId {request.LabelGroupId} combination found in Delete request"
                );
            }

            ProjectDbContext.LabelGroupAssociations.Remove(labelGroupAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(new());
        }
    }
}