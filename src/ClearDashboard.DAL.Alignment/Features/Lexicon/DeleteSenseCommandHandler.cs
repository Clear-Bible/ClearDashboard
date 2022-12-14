using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteSenseCommandHandler : ProjectDbContextCommandHandler<
        DeleteSenseCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteSenseCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteSenseCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteSenseCommand request,
            CancellationToken cancellationToken)
        {
            var sense = ProjectDbContext!.Lexicon_Senses.FirstOrDefault(s => s.Id == request.SenseId.Id);
            if (sense == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid SenseId '{request.SenseId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(sense);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}