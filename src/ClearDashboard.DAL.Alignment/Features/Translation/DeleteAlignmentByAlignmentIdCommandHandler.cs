using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ModelVerificationType = ClearDashboard.DataAccessLayer.Models.AlignmentVerification;
using ModelOriginatedType = ClearDashboard.DataAccessLayer.Models.AlignmentOriginatedFrom;
using ClearDashboard.DAL.Alignment.Features.Events;
using Microsoft.EntityFrameworkCore.Storage;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class DeleteAlignmentByAlignmentIdCommandHandler : ProjectDbContextCommandHandler<DeleteAlignmentByAlignmentIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteAlignmentByAlignmentIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteAlignmentByAlignmentIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteAlignmentByAlignmentIdCommand request,
            CancellationToken cancellationToken)
        {
            var alignment= ProjectDbContext!.Alignments
                .Where(e => e.Id == request.AlignmentId.Id)
                .FirstOrDefault();

            if (alignment == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid AlignmentId '{request.AlignmentId.Id}' found in request"
                );
            }

            if (alignment.Deleted is null)
            {
                alignment.Deleted = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
                var alignmentsToRemove = new List<Models.Alignment>() { alignment };

                using (var transaction = ProjectDbContext.Database.BeginTransaction())
                {
                    alignment.AddDomainEvent(new AlignmentAddingRemovingEvent(
                        alignmentsToRemove, 
                        null, 
                        ProjectDbContext));

                    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                    transaction.Commit();
                }

                await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsToRemove, alignment), cancellationToken);
            }

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}