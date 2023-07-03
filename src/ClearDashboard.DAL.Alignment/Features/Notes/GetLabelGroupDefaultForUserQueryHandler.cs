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
    public class GetLabelGroupDefaultForUserQueryHandler : ProjectDbContextQueryHandler<
        GetLabelGroupDefaultForUserQuery,
        RequestResult<LabelGroupId?>,
        LabelGroupId?>
    {
        private readonly IMediator _mediator;

        public GetLabelGroupDefaultForUserQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLabelGroupDefaultForUserQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LabelGroupId?>> GetDataAsync(GetLabelGroupDefaultForUserQuery request, CancellationToken cancellationToken)
        {
            var user = await ProjectDbContext!.Users
                .FirstOrDefaultAsync(i => i.Id == request.UserId.Id, cancellationToken: cancellationToken);

            if (user == null)
            {
                return new RequestResult<LabelGroupId?>
                (
                    success: false,
                    message: $"User not found for UserId '{request.UserId.Id}'"
                );
            }

            return new RequestResult<LabelGroupId?>
            (
                user.DefaultLabelGroupId is not null
                    ? new LabelGroupId((Guid)user.DefaultLabelGroupId)
                    : null
            );
        }
    }
}
