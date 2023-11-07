
namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public class ResolveExternalNoteCommandParam
    {
        public string ExternalNoteId { get; set; }
        public string ExternalProjectId { get; set; }
        public string VerseRefString { get; set; }
     }
}
