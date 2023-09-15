using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Paratext.PluginInterfaces;
using SIL.Linq;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public static class Extensions
    {
        public static ExternalNote GetExternalNote(this IProjectNote projectNote, IProject project)
        {
            var verseRef = new VerseRef(projectNote.Anchor.VerseRefStart.BookNum, projectNote.Anchor.VerseRefStart.ChapterNum, projectNote.Anchor.VerseRefStart.VerseNum);
            var (versePlainText, plainTextTokensWithIndexes) = project.GetUSFMTokens(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum)
                .GetPlainTextTokensAndIndexes();

            var tokenOfLastSmallerOrEqualUsfmIndex = plainTextTokensWithIndexes
                    .OrderBy(i => i.indexOfTokenInVerseRawUsfm)
                    .Where(i => i.indexOfTokenInVerseRawUsfm < projectNote.Anchor.Offset)
                    .Last();

            return new ExternalNote()
            {
                ExternalNoteType = ExternalNoteType.ParatextNote,
                VersePlainText = versePlainText,
                SelectedPlainText = projectNote.Anchor.SelectedText,
                IndexOfSelectedPlainTextInVersePainText = tokenOfLastSmallerOrEqualUsfmIndex.indexOfTokenInVersePlainText + 
                    (projectNote.Anchor.Offset - tokenOfLastSmallerOrEqualUsfmIndex.indexOfTokenInVerseRawUsfm),
                VerseRefString = verseRef.ToString(),
                ExternalNoteBody = projectNote.GetProjectNoteBody()
            };
        }

        private static string GetProjectNoteBody(this IProjectNote projectNote)
        {
            return "";
        }
        public static (string versePlainText, List<(string tokenPlainText, int indexOfTokenInVersePlainText, int indexOfTokenInVerseRawUsfm)>) GetPlainTextTokensAndIndexes(
            this IEnumerable<IUSFMToken> usfmTokens)
        {
            var plainTextTokenTuples = new List<(string tokenPlainText, int indexOfTokenInVersePlainText, int indexOfTokenInVerseRawUsfm)>();
            var versePlainText = new StringBuilder();
            int indexInPlainText = 0;
            
            usfmTokens.ForEach(usfmToken =>
            {
                if (usfmToken is IUSFMTextToken textToken &&
                    textToken.IsScripture)
                {
                    indexInPlainText += textToken.Text.TrimStart().Length;
                    plainTextTokenTuples.Add((textToken.Text, indexInPlainText, textToken.VerseOffset));
                }
            });
            return (versePlainText.ToString(), plainTextTokenTuples);
        }
    }
}
