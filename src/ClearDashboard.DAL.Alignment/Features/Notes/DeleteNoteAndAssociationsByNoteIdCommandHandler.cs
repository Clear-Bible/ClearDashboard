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
    public class DeleteNoteAndAssociationsByNoteIdCommandHandler : ProjectDbContextCommandHandler<DeleteNoteAndAssociationsByNoteIdCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteNoteAndAssociationsByNoteIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteNoteAndAssociationsByNoteIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteNoteAndAssociationsByNoteIdCommand request,
            CancellationToken cancellationToken)
        {
            // FIXME:  If a label has no other associations than with
            // this note we are deleting, should we delete the label too?
            // Or can they exist and be managed on their own?  
            var note = ProjectDbContext!.Notes.FirstOrDefault(c => c.Id == request.NoteId.Id);
            if (note == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid NoteId '{request.NoteId.Id}' found in request"
                );
            }

            // The data model should be set up to do a cascade delete of
            // any LabelNoteAssociations and/or NoteDomainEntityAssociations
            // when the following executes:
            ProjectDbContext.Remove(note);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new());
        }
    }
}