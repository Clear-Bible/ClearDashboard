using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.Collaboration.Merge;

internal class VerseRowText : ScriptureText
{
    private IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)> _verseRows;

    public VerseRowText(string bookId, ScrVers versification, IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)> verseRows)
        : base(bookId, versification)
    {
        _verseRows = verseRows;
    }

    protected override IEnumerable<TextRow> GetVersesInDocOrder()
    {
        var verses = _verseRows
            .SelectMany(vr => CreateRows(vr.chapter, vr.verse, vr.text, vr.isSentenceStart));

        return verses;
    }
}
