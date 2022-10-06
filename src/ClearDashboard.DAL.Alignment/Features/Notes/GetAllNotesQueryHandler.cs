using ClearBible.Engine.Utils;
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
     public class GetAllNotesQueryHandler : ProjectDbContextQueryHandler<GetAllNotesQuery,
        RequestResult<IEnumerable<Note>>, IEnumerable<Note>>
    {
        private readonly IMediator _mediator;

        public GetAllNotesQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllNotesQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Note>>> GetDataAsync(GetAllNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = ProjectDbContext.Notes
                .Include(n => n.NoteDomainEntityAssociations)
                .Include(n => n.LabelNoteAssociations)
                    .ThenInclude(ln => ln.Label)
                .Include(n => n!.User);

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Note>>(
                notes.Select(note =>
                    new Note(
                        new NoteId(
                            note.Id,
                            note.Created,
                            note.Modified,
                            ModelHelper.BuildUserId(note.User!)),
                        note.Text!,
                        note.AbbreviatedText,
                        note.LabelNoteAssociations
                            .Select(ln => new Label(new LabelId(ln.Label!.Id), ln.Label!.Text ?? string.Empty)).ToHashSet(),
                        note.NoteDomainEntityAssociations
                            .Select(nd => nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!)).ToHashSet()
                    )
                ).ToList()
            );
        }
    }
}
