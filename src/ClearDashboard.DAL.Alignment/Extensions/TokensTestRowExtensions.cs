using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Extensions
{
    public static  class TokensTestRowExtensions
    {
        public static IEnumerable<VerseTokens> CreateVerseTokens(this IEnumerable<TokensTextRow> tokensTextRows)
        {
            return (from row in tokensTextRows
                let firstToken = row.Tokens.FirstOrDefault()
                where firstToken != null
                let tokenId = firstToken.TokenId
                select
                    new VerseTokens(
                        tokenId.ChapterNumber.ToString(), 
                        tokenId.VerseNumber.ToString(), 
                        row.Tokens,
                        row.IsSentenceStart,
                        row.IsInRange,
                        row.IsRangeStart,
                        row.IsEmpty,
                        row.OriginalText));
        }
    }
}
