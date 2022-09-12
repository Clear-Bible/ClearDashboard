using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
     public class GetAllNoteDomainEntityAssociationsQueryHandler : ProjectDbContextQueryHandler<GetAllNoteDomainEntityAssociationsQuery,
        RequestResult<IEnumerable<NoteDomainEntityAssociation>>, IEnumerable<NoteDomainEntityAssociation>>
    {
        private readonly IMediator _mediator;

        public GetAllNoteDomainEntityAssociationsQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetAllNoteDomainEntityAssociationsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<NoteDomainEntityAssociation>>> GetDataAsync(GetAllNoteDomainEntityAssociationsQuery request, CancellationToken cancellationToken)
        {
            var na = ProjectDbContext.NoteDomainEntityAssociations
                .Include(na => na.Note)
                .Select(na => new NoteDomainEntityAssociation(
                    _mediator,
                    new NoteId(na.NoteId, na.Note!.Created, na.Note.Modified, new UserId(na.Note.UserId)),
                    JsonSerializer.Deserialize(na.DomainEntityIdString!, Type.GetType(na.DomainEntityIdTypeString!)!, null as JsonSerializerOptions)!,
                    (na.DomainSubEntityIdString != null) ?
                        JsonSerializer.Deserialize(na.DomainSubEntityIdString!, Type.GetType(na.DomainSubEntityIdTypeString!)!, null as JsonSerializerOptions) :
                        null
                ));
 
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<NoteDomainEntityAssociation>>(na);
        }
    }
}
