using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.User
{
    public record GetCurrentParatextUserQuery() : IRequest<RequestResult<AssignedUser>>;
}
