using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteDefinitionCommandHandler : ProjectDbContextCommandHandler<
        DeleteDefinitionCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteDefinitionCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteDefinitionCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteDefinitionCommand request,
            CancellationToken cancellationToken)
        {
            var definition = ProjectDbContext!.Lexicon_Definitions.FirstOrDefault(d => d.Id == request.DefinitionId.Id);
            if (definition == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid DefinitionId '{request.DefinitionId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(definition);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}