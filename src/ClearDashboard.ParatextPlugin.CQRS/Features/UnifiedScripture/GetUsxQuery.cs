using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;

public record GetUsxQuery(int? BookNumber) : IRequest<RequestResult<StringObject>>
{
    public int? BookNumber { get; } = BookNumber;
}