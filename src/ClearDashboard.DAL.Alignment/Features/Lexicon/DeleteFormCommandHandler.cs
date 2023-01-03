using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteFormCommandHandler : ProjectDbContextCommandHandler<
        DeleteFormCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteFormCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteFormCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteFormCommand request,
            CancellationToken cancellationToken)
        {
            var form = ProjectDbContext!.Lexicon_Forms.FirstOrDefault(f => f.Id == request.FormId.Id);
            if (form == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid FormId '{request.FormId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(form);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}