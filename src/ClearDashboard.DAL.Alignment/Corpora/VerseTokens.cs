using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record VerseTokens(
        string Chapter, 
        string Verse, 
        IEnumerable<Token> Tokens, 
        bool IsSentenceStart, 
        bool IsInRange,
        bool IsRangeStart,
        bool IsEmpty,
        string OriginalText);
}
