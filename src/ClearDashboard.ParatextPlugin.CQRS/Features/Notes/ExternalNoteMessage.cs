using System;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public class ExternalNoteMessage
    {
        public DateTime Created { get; set; }
        public string Language { get; set; }
        public string ExternalUserNameAuthoredBy { get; set; }
        public string Text { get; set; }
    }
}
