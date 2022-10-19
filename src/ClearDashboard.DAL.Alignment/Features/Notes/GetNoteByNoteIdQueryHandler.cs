using ClearBible.Engine.Utils;
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
    public class GetNoteByNoteIdQueryHandler : ProjectDbContextQueryHandler<
        GetNoteByNoteIdQuery,
        RequestResult<Note>,
        Note>
    {
        private readonly IMediator _mediator;

        public GetNoteByNoteIdQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetNoteByNoteIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Note>> GetDataAsync(GetNoteByNoteIdQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var note = ModelHelper.AddIdIncludesNotesQuery(ProjectDbContext)
                .Include(n => n.NoteDomainEntityAssociations)
                .Include(n => n.LabelNoteAssociations)
                    .ThenInclude(ln => ln.Label)
                .FirstOrDefault(pc => pc.Id == request.NoteId.Id);
            if (note == null)
            {
                return new RequestResult<Note>
                (
                    success: false,
                    message: $"Note not found for NotelId '{request.NoteId.Id}'"
                );
            }

            return new RequestResult<Note>
            (
                ModelHelper.BuildNote(note)
            );
        }
    }
}
