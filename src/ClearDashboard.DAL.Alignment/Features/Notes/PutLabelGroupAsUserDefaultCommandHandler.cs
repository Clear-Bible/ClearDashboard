using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class PutLabelGroupAsUserDefaultCommandHandler : ProjectDbContextCommandHandler<
        PutLabelGroupAsUserDefaultCommand,
        RequestResult<Unit>, 
        Unit>
    {
        public PutLabelGroupAsUserDefaultCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutLabelGroupAsUserDefaultCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(PutLabelGroupAsUserDefaultCommand request, CancellationToken cancellationToken)
        {
            var user = await ProjectDbContext!.Users
                .FirstOrDefaultAsync(i => i.Id == request.UserId.Id, cancellationToken: cancellationToken);

            if (user == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"User not found for UserId '{request.UserId.Id}'"
                );
            }

            if (request.LabelGroupId is not null)
            {
                var labelGroup = await ProjectDbContext!.LabelGroups
                    .FirstOrDefaultAsync(i => i.Id == request.LabelGroupId.Id, cancellationToken: cancellationToken);

                if (labelGroup == null)
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"LabelGroup not found for LabelGroupId '{request.LabelGroupId.Id}'"
                    );
                }

                user.DefaultLabelGroupId = request.LabelGroupId.Id;
            }
            else
            {
                user.DefaultLabelGroupId = null;
            }

            await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}
