using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateTokenizedCorpusFromTokenizedCorpusCommand(string ProjectName, ITextCorpus TextCorpus) : ProjectRequestCommand<TokenizedTextCorpus>(ProjectName);
}
