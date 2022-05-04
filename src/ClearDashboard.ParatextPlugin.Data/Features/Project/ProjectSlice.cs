using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.Data.Features.Project
{
    public record GetCurrentProjectCommand() : IRequest<QueryResult<DataAccessLayer.Models.Project>>;
}
