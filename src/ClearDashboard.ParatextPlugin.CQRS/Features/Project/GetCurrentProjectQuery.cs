using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Project
{
    public record GetCurrentProjectQuery() : IRequest<RequestResult<DataAccessLayer.Models.ParatextProject>>;
}
