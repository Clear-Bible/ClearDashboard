using System.Collections.Generic;
using System.Linq;

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
    public HashSet<int> ExternalLabelIds { get; set; }
    public HashSet<ExternalLabel> ExternalLabels { get; set; }

    /// <summary>
    /// includes external note's inner details, the contents of which is not compatible between different external notes systems.
    /// PLEASE DO NOT use in Dashboard UI implementation. If more fields are needed by ui -- AND are likely applicable to external systems
    /// other than Paratext 9 - revise ExternalNote instead.
    /// </summary>
    public string BodyXml { get; set; }

    public string ExternalUserNameAssignedTo { get; set; }

    public bool IsResolved { get; set; }

    public List<ExternalNoteMessage> ExternalNoteMessages { get; set; }

    public string ExternalNoteMessagesString => ExternalNoteMessages
            .Aggregate("", (str, next) => $"{str}Author {next.ExternalUserNameAuthoredBy} {next.Created}:\n{next.Text}\n\n");
}

