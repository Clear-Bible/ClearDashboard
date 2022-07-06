using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateTokenizedCorpusFromTokenizedCorpusCommand(TokenizedTextCorpus TokenizedTextCorpus) : ProjectRequestCommand<TokenizedTextCorpus>;
}
