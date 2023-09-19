namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes;

public class ExternalNote
{
    public string VersePlainText { get; set; }
    public string SelectedPlainText { get; set; }
    /// <summary>
    /// Null means it is a note that applies to the entire verse.
    /// </summary>
    public int? IndexOfSelectedPlainTextInVersePainText { get; set; }
    public string VerseRefString { get; set; }
    public string Body { get; set; }
}

