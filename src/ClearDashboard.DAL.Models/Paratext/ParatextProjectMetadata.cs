
using System.Drawing;
using SIL.Scripture;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParatextProjectMetadata
    {
        
        public string? Id { get; set; }
        public CorpusType CorpusType { get; set; }
        public string? Name { get; set; }
        public string? LongName { get; set; }
        public string? CombinedName => string.Format("{0}, {1}", Name, LongName);
        public string? LanguageName { get; set; }
        /// <summary>
        /// IETF BCP47 language tag
        /// </summary>
        public string? LanguageId { get; set; }
        public string? ProjectPath { get; set; }
        public bool IsRtl { get; set; }
        public string FontFamily { get; set; } = "Segoe UI";

        public bool HasProjectPath => !string.IsNullOrEmpty(ProjectPath);

        public string CorpusTypeDisplay => CorpusType.ToString();

        //public Dictionary<string, string> BookChapterVerseDictionary { get; set; } = new Dictionary<string, string>();

        public List<BookInfo> AvailableBooks { get; set; } = new List<BookInfo>();

        public ScrVers ScrVers { get; set; }
    }
}
