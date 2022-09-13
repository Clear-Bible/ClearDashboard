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
     public class GetLabelNoteAssociationsByLabelIdOrNoteIdQueryHandler : ProjectDbContextQueryHandler<GetLabelNoteAssociationsByLabelIdOrNoteIdQuery,
        RequestResult<IEnumerable<LabelNoteAssociation>>, IEnumerable<LabelNoteAssociation>>
    {
        private readonly IMediator _mediator;

        public GetLabelNoteAssociationsByLabelIdOrNoteIdQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLabelNoteAssociationsByLabelIdOrNoteIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<LabelNoteAssociation>>> GetDataAsync(GetLabelNoteAssociationsByLabelIdOrNoteIdQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Models.LabelNoteAssociation> ln = ProjectDbContext.LabelNoteAssociations;
            if (request.LabelId != null)
            {
                ln = ln.Include(ln => ln.Label).Where(ln => ln.LabelId == request.LabelId.Id);
            }
            if (request.NoteId != null)
            {
                ln = ln.Include(ln => ln.Note).Where(ln => ln.NoteId == request.NoteId.Id);
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<LabelNoteAssociation>>(ln.Select(ln =>
                new LabelNoteAssociation(
                    _mediator,
                    new LabelNoteAssociationId(ln.Id),
                    new LabelId(ln.LabelId),
                    new NoteId(ln.NoteId, ln.Note!.Created, ln.Note.Modified, new UserId(ln.Note.UserId))
                )));
        }
    }
}
