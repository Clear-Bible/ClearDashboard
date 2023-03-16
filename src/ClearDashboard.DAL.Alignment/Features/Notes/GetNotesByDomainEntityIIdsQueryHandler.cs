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
     public class GetNotesByDomainEntityIIdsQueryHandler : ProjectDbContextQueryHandler<GetNotesByDomainEntityIIdsQuery,
        RequestResult<IEnumerable<Note>>, IEnumerable<Note>>
    {
        public GetNotesByDomainEntityIIdsQueryHandler( 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetNotesByDomainEntityIIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Note>>> GetDataAsync(GetNotesByDomainEntityIIdsQuery request, CancellationToken cancellationToken)
        {
            var dbNotes = ModelHelper.AddIdIncludesNotesQuery(ProjectDbContext)
                .Include(n => n.NoteDomainEntityAssociations)
                .Include(n => n.LabelNoteAssociations)
                    .ThenInclude(ln => ln.Label)
                .Include(n => n.NoteUserSeenAssociations)
                .AsQueryable();

            if (request.DomainEntityIIds is not null && request.DomainEntityIIds.Any())
            {
                var filterIds = request.DomainEntityIIds.Select(d => d.Id).ToList();
                dbNotes = dbNotes
                    .Where(n => n.NoteDomainEntityAssociations
                        .Where(dea => dea.DomainEntityIdGuid != null)
                        .Where(dea => filterIds.Contains((Guid)dea.DomainEntityIdGuid!))
                        .Any());
            }

            var notes = dbNotes.Select(n => ModelHelper.BuildNote(n)).ToList();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Note>>(notes);
        }
    }
}
