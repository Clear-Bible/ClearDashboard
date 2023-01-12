using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Project;

public record GetProjectFontFamilyQuery(string ParatextProjectId) : IRequest<RequestResult<string>>
{
    public string ParatextProjectId { get; } = ParatextProjectId;
}