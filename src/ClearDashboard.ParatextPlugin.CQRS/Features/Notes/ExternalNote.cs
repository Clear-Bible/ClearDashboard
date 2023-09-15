namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes;

public class ExternalNote
{
    public string VersePlainText { get; set; }
    public string SelectedPlainText { get; set; }
    public int IndexOfSelectedPlainTextInVersePainText { get; set; }
    public string VerseRefString { get; set; }
    public string Body { get; set; }
}

