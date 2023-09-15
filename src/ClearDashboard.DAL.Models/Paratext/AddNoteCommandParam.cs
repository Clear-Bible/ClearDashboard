
namespace ClearDashboard.DataAccessLayer.Models
{
    public class AddNoteCommandParam 
    {
        /// <summary>
        /// Must be set to a valid externalprojectid or plugin will exception.
        /// </summary>
        public string ExternalProjectId { get; set; } = string.Empty;

        /// <summary>
        /// if Book, Chapter, and Verse are not all set to a positive value then will attempt to apply note to the current verse paratext is set to.
        /// </summary>
        public int Book { get; set; } = -1;
        /// <summary>
        /// if Book, Chapter, and Verse are not all set to a positive value then will attempt to apply note to the current verse paratext is set to.
        /// </summary>
        public int Chapter { get; set; } = -1;
        /// <summary>
        /// if Book, Chapter, and Verse are not all set to a positive value then will attempt to apply note to the current verse paratext is set to.
        /// </summary>
        public int Verse { get; set; } = -1;

        /// <summary>
        /// if empty, apply note to entire verse
        /// </summary>
        public string SelectedText { get; set; } = string.Empty;
        public int OccuranceIndexOfSelectedTextInVerseText { get; set; } = -1;
        public List<string> NoteParagraphs { get; set; } = new List<string>();
        public string? ParatextUser { get; set; }
    }
}
