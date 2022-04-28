using MediatR;

namespace ParaTextPlugin.Data.Features.Project
{
    public record GetCurrentProjectCommand() : IRequest<QueryResult<Models.Project>>;
}
