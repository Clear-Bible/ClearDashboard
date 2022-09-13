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
    public class DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommandHandler : ProjectDbContextCommandHandler<DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommand request,
            CancellationToken cancellationToken)
        {
            var noteDomainEntityAssociation = ProjectDbContext!.NoteDomainEntityAssociations.FirstOrDefault(ln => ln.Id == request.NoteDomainEntityAssociationId.Id);
            if (noteDomainEntityAssociation == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid NoteDomainEntityAssociationId '{request.NoteDomainEntityAssociationId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(noteDomainEntityAssociation);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new ());
        }
    }
}