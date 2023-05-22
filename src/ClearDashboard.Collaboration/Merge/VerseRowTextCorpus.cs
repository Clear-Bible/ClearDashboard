using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.Collaboration.Merge;

internal class VerseRowTextCorpus : ScriptureTextCorpus
{
    internal VerseRowTextCorpus(ScrVers versification, IEnumerable<VerseRowText> verseRowTexts): base()
    {
        Versification = versification;
        foreach (var verseRowText in verseRowTexts)
        {
            AddText(verseRowText);
        }
    }
}

