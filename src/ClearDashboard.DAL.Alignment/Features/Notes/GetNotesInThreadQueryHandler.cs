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
using System.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
     public class GetNotesInThreadQueryHandler : ProjectDbContextQueryHandler<GetNotesInThreadQuery,
        RequestResult<IOrderedEnumerable<Note>>, IOrderedEnumerable<Note>>
    {
        private readonly IMediator _mediator;

        public GetNotesInThreadQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetNotesInThreadQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IOrderedEnumerable<Note>>> GetDataAsync(GetNotesInThreadQuery request, CancellationToken cancellationToken)
        {
            var notes = ModelHelper.AddIdIncludesNotesQuery(ProjectDbContext)
                .Include(n => n.NoteDomainEntityAssociations)
                .Include(n => n.LabelNoteAssociations)
                    .ThenInclude(ln => ln.Label)
                .Where(n => request.ThreadNoteId.Id == n.Id || request.ThreadNoteId.Id == n.ThreadId)
                .Select(note => ModelHelper.BuildNote(note));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IOrderedEnumerable<Note>>(
                notes
                    .ToList()
                    .OrderBy(n => n.NoteId!.IdEquals(n.ThreadId ?? n.NoteId!))
                    .OrderBy(n => n.NoteId!.Created)
            );
        }
    }
}
