using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record ImportWordAnalysesCommand(
        IEnumerable<WordAnalysis> WordAnalyses,
        TokenizedTextCorpusId TokenizedTextCorpusId) : ProjectRequestCommand<Unit>;
}
