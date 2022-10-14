using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.VerseText
{
    public record GetCurrentParatextVerseTextQuery() : IRequest<RequestResult<AssignedUser>>;
}
