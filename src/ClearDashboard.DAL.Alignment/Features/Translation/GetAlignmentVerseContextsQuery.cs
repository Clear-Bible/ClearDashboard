using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentVerseContextsQuery(AlignmentSetId AlignmentSetId, string sourceTokenTrainingText, string targetTokenTrainingText) : 
        ProjectRequestQuery<IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceTokenTrainingTextVerseTokens,
            uint sourceTokenTrainingTextTokensIndex,
            IEnumerable<Token> targetTokenTrainingTextTargetVerseTokens,
            uint targetTokenTrainingTextTokensIndex
        )>>;
}
