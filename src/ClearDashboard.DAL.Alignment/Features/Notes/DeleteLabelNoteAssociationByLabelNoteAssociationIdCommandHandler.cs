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
    public class DeleteLabelNoteAssociationByLabelNoteAssociationIdCommandHandler : ProjectDbContextCommandHandler<DeleteLabelNoteAssociationByLabelNoteAssociationIdCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteLabelNoteAssociationByLabelNoteAssociationIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteLabelNoteAssociationByLabelNoteAssociationIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteLabelNoteAssociationByLabelNoteAssociationIdCommand request,
            CancellationToken cancellationToken)
        {
            var labelNoteAssociation = ProjectDbContext!.LabelNoteAssociations.FirstOrDefault(ln => ln.Id == request.LabelNoteAssociationId.Id);
            if (labelNoteAssociation == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid LabelNoteAssociationId '{request.LabelNoteAssociationId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(labelNoteAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new ());
        }
    }
}