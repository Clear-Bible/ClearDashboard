using SIL.Scripture;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
public enum ExternalNoteType
{
    ParatextNote
}

public class ExternalNote
{
    public ExternalNoteType ExternalNoteType { get; set; }
    public string VersePlainText { get; set; }
    public string SelectedPlainText { get; set; }
    public int IndexOfSelectedPlainTextInVersePainText { get; set; }
    public string VerseRefString { get; set; }
    public string ExternalNoteBody { get; set; }
}

