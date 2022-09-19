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
    public class DeleteLabelNoteAssociationCommandHandler : ProjectDbContextCommandHandler<DeleteLabelNoteAssociationCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteLabelNoteAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteLabelNoteAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteLabelNoteAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var labelNoteAssociation = ProjectDbContext!.LabelNoteAssociations.FirstOrDefault(ln => 
                ln.LabelId == request.LabelId.Id && ln.NoteId == request.NoteId.Id);
            if (labelNoteAssociation == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid LabelId '{request.LabelId}' / NoteId {request.NoteId} combination found in request"
                );
            }

            ProjectDbContext.Remove(labelNoteAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new ());
        }
    }
}