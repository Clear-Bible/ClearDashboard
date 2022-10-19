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
        public GetAllNotesQueryHandler( 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllNotesQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Note>>> GetDataAsync(GetAllNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = ModelHelper.AddIdIncludesNotesQuery(ProjectDbContext)
                .Include(n => n.NoteDomainEntityAssociations)
                .Include(n => n.LabelNoteAssociations)
                    .ThenInclude(ln => ln.Label)
                .Select(note => ModelHelper.BuildNote(note));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Note>>(
                notes
                    .ToList()
            );
        }
    }
}
