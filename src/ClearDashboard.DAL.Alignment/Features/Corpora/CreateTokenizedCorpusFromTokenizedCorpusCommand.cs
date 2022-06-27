using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateTokenizedCorpusFromTokenizedCorpusCommand(ITextCorpus textCorpus) : IRequest<RequestResult<TokenizedTextCorpus>>;
}
