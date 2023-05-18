using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class DeleteLabelGroupAndAssociationsByLabelGroupIdCommandHandler : ProjectDbContextCommandHandler<DeleteLabelGroupAndAssociationsByLabelGroupIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteLabelGroupAndAssociationsByLabelGroupIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteLabelGroupAndAssociationsByLabelGroupIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteLabelGroupAndAssociationsByLabelGroupIdCommand request,
            CancellationToken cancellationToken)
        {
            var labelGroup = ProjectDbContext!.LabelGroups.FirstOrDefault(c => c.Id == request.LabelGroupId.Id);
            if (labelGroup == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid LabelGroupId '{request.LabelGroupId.Id}' found in Delete request"
                );
            }

            // The data model should be set up to do a cascade delete of
            // any LabelGroupAssociations when the following executes:
            ProjectDbContext.LabelGroups.Remove(labelGroup);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(new());
        }
    }
}