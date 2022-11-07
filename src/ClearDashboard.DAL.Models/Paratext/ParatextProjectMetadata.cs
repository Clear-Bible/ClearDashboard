
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
        public string? LanguageName { get; set; }
        public string? ProjectPath { get; set; }
        public bool IsRtl { get; set; }
        public FontFamily FontFamily { get; set; } = new FontFamily("Roboto");

        public bool HasProjectPath => !string.IsNullOrEmpty(ProjectPath);

        public string CorpusTypeDisplay => CorpusType.ToString();

        //public Dictionary<string, string> BookChapterVerseDictionary { get; set; } = new Dictionary<string, string>();

        public List<BookInfo> AvailableBooks { get; set; } = new List<BookInfo>();

        public ScrVers ScrVers { get; set; }
    }
}
