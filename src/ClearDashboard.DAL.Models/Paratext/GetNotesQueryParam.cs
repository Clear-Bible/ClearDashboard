
namespace ClearDashboard.DataAccessLayer.Models
{
    public class GetNotesQueryParam 
    {
        /// <summary>
        /// Must be set to a valid externalprojectid or plugin will exception.
        /// </summary>
        public string ExternalProjectId { get; set; } = string.Empty;
        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
        public bool IncludeResolved { get; set; }
    }
}
