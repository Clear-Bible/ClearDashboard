using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetExternalLexiconQuery(string? ProjectId) : IRequest<RequestResult<Alignment.Lexicon.Lexicon>>;
}
