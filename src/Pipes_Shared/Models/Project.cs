using System.Collections.Generic;

namespace ParaTextPlugin.Data.Models
{
    public class Project : BindableBase
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



        private string _id = "";
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _shortName = "";
        public string ShortName
        {
            get => _shortName;
            set => SetProperty(ref _shortName, value);
        }


        private string _longName = "";
        public string LongName
        {
            get => _longName;
            set => SetProperty(ref _longName, value, nameof(LongName));
        }


        private string _languageName = "";
        public string LanguageName
        {
            get => _languageName;
            set => SetProperty(ref _languageName, value);
        }

        private List<BookInfo> _availableBooks = new List<BookInfo>();
        public List<BookInfo> AvailableBooks
        {
            get => _availableBooks;
            set => SetProperty(ref _availableBooks, value, nameof(AvailableBooks));
        }


        private List<string> _nonObservers = new List<string>();
        public List<string> NonObservers
        {
            get => _nonObservers;
            set => SetProperty(ref _nonObservers, value, nameof(NonObservers));
        }


        private ProjectType _type;
        public ProjectType Type
        {
            get => _type;
            set => SetProperty(ref _type, value, nameof(Type));
        }


        private ScrLanguageWrapper _language;
        public ScrLanguageWrapper Language
        {
            get => _language;
            set => SetProperty(ref _language, value, nameof(Language));
        }


        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set => SetProperty(ref _bcvDictionary, value, nameof(BcvDictionary));
        }

    }
}
