using ClearBible.Engine.Utils;
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
    public class DeleteNoteDomainEntityAssociationCommandHandler : ProjectDbContextCommandHandler<DeleteNoteDomainEntityAssociationCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteNoteDomainEntityAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteNoteDomainEntityAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteNoteDomainEntityAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var (name, guid) = request.DomainEntityId.GetNameAndId();

            var noteDomainEntityAssociation = ProjectDbContext!.NoteDomainEntityAssociations.FirstOrDefault(ln => 
                ln.NoteId == request.NoteId.Id && ln.DomainEntityIdGuid == guid);

            if (noteDomainEntityAssociation == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid NoteId '{request.NoteId}' / DomainEntityIdGuid '{guid}' combination found in request"
                );
            }

            ProjectDbContext.Remove(noteDomainEntityAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new());
        }
    }
}