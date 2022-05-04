using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Project
{
    public record GetCurrentProjectCommand() : IRequest<QueryResult<DataAccessLayer.Models.Project>>;
}
