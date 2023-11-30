using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes;

public class ExternalNote
{
    public string ExternalNoteId { get; set; }
    public string ExternalProjectId { get; set; }
    public string VersePlainText { get; set; }
    public string SelectedPlainText { get; set; }
    /// <summary>
    /// Null means it is a note that applies to the entire verse.
    /// </summary>
    public int? IndexOfSelectedPlainTextInVersePainText { get; set; }
    public string VerseRefString { get; set; }
    public string Message { get; set; }
    public string Body { get; set; }
    public HashSet<int> ExternalLabelIds { get; set; }
    public HashSet<ExternalLabel> ExternalLabels { get; set; }
}

