
namespace ClearDashboard.DataAccessLayer.Models
{
    /// <summary>
    /// Clear.Bible verse location information and conversion.
    /// </summary>
    public class BookChapterVerse
    {

        public List<string>? BibleBookList { get; set; } = new List<string>();

        public string BookAbbr { get; set; } = string.Empty;

        public List<int>? ChapterNumbers { get; set; } = new List<int>();


        public List<int>? VerseNumbers { get; set; } = new List<int>();

   
        /// <summary>
        /// The Book ID number as an int.
        /// </summary>
        public int BookNum { get; set; } = 1;

      

        /// <summary>
        /// The book name. Defaults to Genesis
        /// </summary>
        public string? BookName { get; set; } = "GEN";

        /// <summary>
        /// Chapter number as an int.
        /// </summary>
        public int? Chapter { get; set; } = 1;

        /// <summary>
        /// The verse number as an int.
        /// </summary>
        public int? Verse { get; set; } = 0;

    }
}
