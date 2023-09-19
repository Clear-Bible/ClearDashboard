using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentVerseContextsQuery(AlignmentSetId AlignmentSetId, string SourceString, string TargetString, bool StringsAreTraining, int? BookNumber, AlignmentTypes AlignmentTypesToInclude, int? Limit, string? Cursor) :
        ProjectRequestQuery<(IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )> VerseContexts, string Cursor, bool HasNextPage)>
    {
        public bool HasLimit()
        {
            return Limit is not null && Limit.HasValue && Limit.Value > 0;
        }

        public bool HasCursor()
        {
            return !string.IsNullOrEmpty(Cursor);
        }
    }
}
