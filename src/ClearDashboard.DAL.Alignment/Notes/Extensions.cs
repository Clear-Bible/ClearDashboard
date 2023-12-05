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
        /// <param name="externalProjectId"></param>
        /// <param name="verseTokens">All tokens must be from the same book, chapter, and verse. Applies note to the first token's book, chapter, verse. </param>
        /// <param name="verseContiguousSelectedTokens">If empty, apply note to entire verse. When non-empty, all tokens must be also included in the prior parameter.</param>        /// <param name="engineStringDetokenizer"></param>
        /// <param name="noteText"></param>
        /// <param name="assignedUser"></param>
        /// <param name="book">if book, chapter, or verse not set the current external setting for the current project will be used.</param>
        /// <param name="chapter">if book, chapter, or verse not set the current external setting for the current project will be used.</param>
        /// <param name="verse">if book, chapter, or verse not set the current external setting for the current project will be used.</param>
        /// <exception cref="Exception"></exception>
        public static void SetProperties(
            this AddNoteCommandParam addNoteCommandParam,
            string externalProjectId,
            IEnumerable<Token> verseTokens,
            IEnumerable<Token> verseContiguousSelectedTokens,
            EngineStringDetokenizer engineStringDetokenizer,
            string noteText,
            int book = -1,
            int chapter = -1,
            int verse = -1,
            string? userName = null)
        {
            if (verseTokens.Count() == 0)
            {
                throw new Exception("Must supply a non-zero amount of verse tokens");
            }

            addNoteCommandParam.ExternalProjectId = externalProjectId;
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

             addNoteCommandParam.UserName = userName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalNotes"></param>
        /// <param name="verseTokens">All the tokens for a single verse. Empty list means applies to the whole verse. Null means the external verse plain text
        /// and verse plain text (verse row) calculated from tokens don't match.</param>
        /// <param name="engineStringDetokenizer"></param>
        /// <returns>
        /// - Zero count of tokenIds with externalNote.IndexOfSelectedPlainTextInVersePainText > 0 means it identifies a location in between tokens. Since
        /// Dashboard doesn't support 'marker' notes this would be rendered as pertaining to the entire verse.
        /// - externalNote.IndexOfSelectedPlainTextInVersePainText == 0 means the note pertains to the verse.
        /// 
        /// </returns>
        public static IEnumerable<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> AddVerseAndTokensContext(
            this IEnumerable<ExternalNote> externalNotes,
            TokensTextRow tokenTextRow,
            EngineStringDetokenizer engineStringDetokenizer)
        {
            var tokensWithPadding = engineStringDetokenizer.Detokenize(tokenTextRow.Tokens.GetPositionalSortedBaseTokens());
            var versePlainText = $"{tokensWithPadding.Aggregate(string.Empty, (constructedString, tokenWithPadding) => $"{constructedString}{tokenWithPadding.paddingBefore}{tokenWithPadding.token}{tokenWithPadding.paddingAfter}")}";
            var verseRef = (VerseRef)tokenTextRow.Ref;

            return externalNotes
                .Select(en =>
                {
                    if (en.VersePlainText != versePlainText)
                    {
                        return (verseRef, null, en);
                    }
                    else
                    {
                        return (
                            verseRef, 
                            GetSelectedTokenIdsForSelection(tokensWithPadding, en.IndexOfSelectedPlainTextInVersePainText, en.SelectedPlainText), 
                            en
                        );
                    }
                });
        }

        private static List<TokenId>? GetSelectedTokenIdsForSelection(
            IEnumerable<(Token token, string paddingBefore, string paddingAfter)> tokensWithPadding, 
            int? indexOfSelectedPlainTextInVersePainText, 
            string selectedPlainText)
        {
            
            var selectedTokenIds = new List<TokenId>();
            if (selectedPlainText == null || selectedPlainText.Length == 0 || indexOfSelectedPlainTextInVersePainText == null)
            {
                return selectedTokenIds;
            }

            int index = 0;
            bool firstTokenFound = false;
            string remainingSelectedPlainText = selectedPlainText;

            foreach (var tokenWithPadding in tokensWithPadding)
            {
                var tokenPaddingBeforeText = $"{tokenWithPadding.paddingBefore}";
                var tokenTextWithoutPaddingBefore = $"{tokenWithPadding.token}{tokenWithPadding.paddingAfter}"; ;
                var tokenText = $"{tokenPaddingBeforeText}{tokenTextWithoutPaddingBefore}";

                if (!firstTokenFound)
                {
                    if (index + tokenPaddingBeforeText.Length < indexOfSelectedPlainTextInVersePainText)
                    {
                        index += tokenText.Length;
                    }
                    else
                    {
                        firstTokenFound = true;
                    }
                }
                
                if (firstTokenFound)
                {
                    if (remainingSelectedPlainText.Contains(tokenTextWithoutPaddingBefore))
                    {
                        selectedTokenIds.Add(tokenWithPadding.token.TokenId);
                        var indexNextToken = remainingSelectedPlainText.IndexOf(tokenTextWithoutPaddingBefore) + tokenTextWithoutPaddingBefore.Length;
                        remainingSelectedPlainText = remainingSelectedPlainText.Substring(indexNextToken, remainingSelectedPlainText.Length - indexNextToken);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return selectedTokenIds;
        }
    }
}
