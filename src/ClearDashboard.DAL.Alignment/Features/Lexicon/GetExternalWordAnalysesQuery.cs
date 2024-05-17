using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetExternalWordAnalysesQuery(string? ProjectId) : IRequest<RequestResult<IEnumerable<Alignment.Lexicon.WordAnalysis>>>;
}
