
namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public class GetNotesQueryParam 
    {
        public string ExternalProjectId { get; set; } = string.Empty;
        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
        public bool IncludeResolved { get; set; }
    }
}
