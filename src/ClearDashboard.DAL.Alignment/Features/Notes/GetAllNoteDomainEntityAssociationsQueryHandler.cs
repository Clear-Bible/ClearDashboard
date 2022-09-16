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
            var nd = ProjectDbContext.NoteDomainEntityAssociations.Include(nd => nd.Note);

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<NoteDomainEntityAssociation>>(nd.Select(nd =>
                new NoteDomainEntityAssociation(
                    _mediator,
                    new NoteDomainEntityAssociationId(nd.Id),
                    new NoteId(nd.NoteId, nd.Note!.Created, nd.Note.Modified, new UserId(nd.Note.UserId)),
                    string.Empty, // FIXME
                    nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!)
                )));
        }
    }
}
