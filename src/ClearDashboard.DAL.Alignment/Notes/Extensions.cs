using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Scripture;
using Token = ClearBible.Engine.Corpora.Token;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addNoteCommandParam"></param>
        /// <param name="paratextProjectId"></param>
        /// <param name="verseTokens">All tokens must be from the same book, chapter, and verse. Applies note to the first token's book, chapter, verse. </param>
        /// <param name="verseContiguousSelectedTokens">If empty, apply note to entire verse. When non-empty, all tokens must be also included in the prior parameter.</param>        /// <param name="engineStringDetokenizer"></param>
        /// <param name="noteText"></param>
        /// <param name="assignedUser"></param>
        /// <param name="book">if book, chapter, or verse not set the current paratext setting for the current project will be used.</param>
        /// <param name="chapter">if book, chapter, or verse not set the current paratext setting for the current project will be used.</param>
        /// <param name="verse">if book, chapter, or verse not set the current paratext setting for the current project will be used.</param>
        /// <exception cref="Exception"></exception>
        public static void SetProperties(
            this AddNoteCommandParam addNoteCommandParam,
            string paratextProjectId,
            IEnumerable<Token> verseTokens,
            IEnumerable<Token> verseContiguousSelectedTokens,
            EngineStringDetokenizer engineStringDetokenizer,
            string noteText,
            int book = -1,
            int chapter = -1,
            int verse = -1,
            User? assignedUser = null)
        {
            if (verseTokens.Count() == 0)
            {
                throw new Exception("Must supply a non-zero amount of verse tokens");
            }

            addNoteCommandParam.ExternalProjectId = paratextProjectId;
            addNoteCommandParam.Book = book;
            addNoteCommandParam.Chapter = chapter;
            addNoteCommandParam.Verse = verse;

            var token = verseTokens.First();
            addNoteCommandParam.Book = token.TokenId.BookNumber;
            addNoteCommandParam.Chapter = token.TokenId.ChapterNumber;
            addNoteCommandParam.Verse = token.TokenId.VerseNumber;

            //addNoteCommandParam.Verse = new VerseRef(int.Parse(verseTokens.First().TokenId.ToString().Substring(0, 9)));
            var verseText = $"{engineStringDetokenizer.Detokenize(verseTokens).Aggregate(string.Empty, (constructedString, tokenWithPadding) => $"{constructedString}{tokenWithPadding.paddingBefore}{tokenWithPadding.token}{tokenWithPadding.paddingAfter}")}";
            var selectedText = verseContiguousSelectedTokens.Count() > 0 ? 
                $"{engineStringDetokenizer.Detokenize(verseContiguousSelectedTokens).Aggregate(string.Empty, (constructedString, tokenWithPadding) => $"{constructedString}{tokenWithPadding.paddingBefore}{tokenWithPadding.token}{tokenWithPadding.paddingAfter}")}"
                : "";

            if (selectedText.Length > 0)
            {
                var tokenIndexBeginOfSelection = verseTokens
                    .Select((token, index) => (token, index))
                    .First(ti => ti.token.TokenId.Equals(verseContiguousSelectedTokens.First().TokenId)).index;

                var offsetOfSelection = $"{engineStringDetokenizer.Detokenize(verseTokens.Take(tokenIndexBeginOfSelection)).Aggregate(string.Empty, (constructedString, tokenWithPadding) => $"{constructedString}{tokenWithPadding.paddingBefore}{tokenWithPadding.token}{tokenWithPadding.paddingAfter}")}"
                        .Length;

                var beforeSelection = verseText.Substring(0, offsetOfSelection);

                int countOfSelectedTextBeforeSelection = 0;
                int i = 0;
                while ((i = beforeSelection.IndexOf(selectedText, i)) != -1)
                {
                    i += selectedText.Length;
                    countOfSelectedTextBeforeSelection++;
                }
                addNoteCommandParam.OccuranceIndexOfSelectedTextInVerseText = countOfSelectedTextBeforeSelection;
            }
            addNoteCommandParam.SelectedText = selectedText;

            //Contents
            var span = new Span();
            span.Text = noteText;

            var content = new Content();
            content.Spans.Add(span);

            addNoteCommandParam.NoteParagraphs = noteText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();

            //ParatextUser
             addNoteCommandParam.ParatextUser = assignedUser?.ParatextUserName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalNotes"></param>
        /// <param name="verseTokens">All the tokens for a single verse</param>
        /// <param name="engineStringDetokenizer"></param>
        /// <returns></returns>
        public static IEnumerable<(VerseRef verseRef, IEnumerable<TokenId> tokenIds, ExternalNote externalNote)> AddVerseAndTokensContext(
            this IEnumerable<ExternalNote> externalNotes,
            IEnumerable<Token> verseTokens,
            EngineStringDetokenizer engineStringDetokenizer)
        {
            throw new Exception();
        }
    }
}
