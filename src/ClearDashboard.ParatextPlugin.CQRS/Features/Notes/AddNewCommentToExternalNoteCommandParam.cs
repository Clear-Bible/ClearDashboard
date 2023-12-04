
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public class AddNewCommentToExternalNoteCommandParam
    {
        public string ExternalNoteId { get; set; }
        public string ExternalProjectId { get; set; }
        public string VerseRefString { get; set; }
        public string Comment { get; set; }
        public string AssignToUserName { get; set; } = string.Empty;
    }
}
