using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public record GetLexiconQuery(string ProjectId) : IRequest<RequestResult<DataAccessLayer.Models.Lexicon_Lexicon>>
    {
        public string ProjectId { get; } = ProjectId;
    }
}
