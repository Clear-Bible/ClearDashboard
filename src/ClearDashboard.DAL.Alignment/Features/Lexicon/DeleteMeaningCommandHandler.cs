using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteMeaningCommandHandler : ProjectDbContextCommandHandler<
        DeleteMeaningCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteMeaningCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteMeaningCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteMeaningCommand request,
            CancellationToken cancellationToken)
        {
            var meaning = ProjectDbContext!.Lexicon_Meanings.FirstOrDefault(s => s.Id == request.MeaningId.Id);
            if (meaning == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid MeaningId '{request.MeaningId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(meaning);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}