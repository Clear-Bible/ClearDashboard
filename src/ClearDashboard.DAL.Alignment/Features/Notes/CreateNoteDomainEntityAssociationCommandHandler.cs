using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class CreateNoteDomainEntityAssociationCommandHandler : ProjectDbContextCommandHandler<CreateNoteDomainEntityAssociationCommand,
        RequestResult<NoteDomainEntityAssociationId>, NoteDomainEntityAssociationId>
    {
        private readonly IMediator _mediator;

        public CreateNoteDomainEntityAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateNoteDomainEntityAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<NoteDomainEntityAssociationId>> SaveDataAsync(CreateNoteDomainEntityAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var noteDomainEntityAssociation = new Models.NoteDomainEntityAssociation
            {
                NoteId = request.NoteId.Id,
                DomainEntityIdString = JsonSerializer.Serialize(request.DomainEntityId),
                DomainEntityIdTypeString = request.DomainEntityId.GetType().FullName
            };

            if (request.DomainSubEntityId != null)
            {
                noteDomainEntityAssociation.DomainSubEntityIdString = JsonSerializer.Serialize(request.DomainSubEntityId);  
                noteDomainEntityAssociation.DomainSubEntityIdTypeString = request.DomainSubEntityId.GetType().FullName;
            }

            ProjectDbContext.NoteDomainEntityAssociations.Add(noteDomainEntityAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<NoteDomainEntityAssociationId>(
                new NoteDomainEntityAssociationId(noteDomainEntityAssociation.Id));
        }
    }
}