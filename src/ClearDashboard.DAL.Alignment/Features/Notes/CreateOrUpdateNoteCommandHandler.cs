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
    public class CreateOrUpdateNoteCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateNoteCommand,
        RequestResult<NoteId>, NoteId>
    {
        private readonly IMediator _mediator;

        public CreateOrUpdateNoteCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateOrUpdateNoteCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<NoteId>> SaveDataAsync(CreateOrUpdateNoteCommand request,
            CancellationToken cancellationToken)
        {
            Models.Note? note = null;
            if (request.NoteId != null)
            {
                note = ProjectDbContext!.Notes.Include(n => n.User).FirstOrDefault(c => c.Id == request.NoteId.Id);
                if (note == null)
                {
                    return new RequestResult<NoteId>
                    (
                        success: false,
                        message: $"Invalid NoteId '{request.NoteId.Id}' found in request"
                    );
                }

                note.Text = request.Text;
                note.AbbreviatedText = request.AbbreviatedText;
                note.Modified = DateTimeOffset.UtcNow;
            }
            else
            {
                note = new Models.Note
                {
                    Id = Guid.NewGuid(),
                    Text = request.Text,
                    AbbreviatedText = request.AbbreviatedText,
                };

                ProjectDbContext.Notes.Add(note);
            }
            
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            note = ProjectDbContext.Notes.Include(n => n.User).First(n => n.Id == note.Id);

            return new RequestResult<NoteId>(new NoteId(note.Id, note.Created, note.Modified, ModelHelper.BuildUserId(note.User!)));
        }
    }
}