using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public record GetLexiconQuery() : IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>>;
}
