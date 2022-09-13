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
    public class DeleteNoteByNoteIdCommandHandler : ProjectDbContextCommandHandler<DeleteNoteByNoteIdCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteNoteByNoteIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteNoteByNoteIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteNoteByNoteIdCommand request,
            CancellationToken cancellationToken)
        {
            var note = ProjectDbContext!.Notes.FirstOrDefault(c => c.Id == request.NoteId.Id);
            if (note == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid NoteId '{request.NoteId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(note);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new ());
        }
    }
}