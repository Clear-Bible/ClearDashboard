

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Project 
    {
        public enum ProjectType
        {
            Standard,
            Resource,
            BackTranslation,
            Daughter,
            TransliterationManual,
            TransliterationWithEncoder,
            StudyBible,
            ConsultantNotes,
            GlobalConsultantNotes,
            GlobalAnthropologyNotes,
            StudyBibleAdditions,
            Auxiliary,
            AuxiliaryResource,
            MarbleResource,
            XmlResource,
            XmlDictionary,
            NotSelected
        }

        public string Id { get; set; }

        public string ShortName { get; set; }

        
        public string LongName { get; set; }

        public string LanguageName { get; set; }

        public List<BookInfo> AvailableBooks { get; set; } = new List<BookInfo>();

        public List<string> NonObservers { get; set; } = new List<string>();

        public ProjectType Type { get; set; }

        public ScrLanguageWrapper Language { get; set; } 

        public Dictionary<string, string> BcvDictionary { get; set; } = new Dictionary<string, string>();

    }
}
