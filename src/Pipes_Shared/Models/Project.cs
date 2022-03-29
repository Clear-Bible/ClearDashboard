using Newtonsoft.Json;
using System.Collections.Generic;

namespace Pipes_Shared.Models
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



        private string _ID = "";
        [JsonProperty]
        public string ID
        {
            get => _ID;
            set { SetProperty(ref _ID, value); }
        }

        private string _ShortName = "";
        [JsonProperty]
        public string ShortName
        {
            get => _ShortName;
            set { SetProperty(ref _ShortName, value); }
        }


        private string _LongName = "";
        [JsonProperty]
        public string LongName
        {
            get => _LongName;
            set { SetProperty(ref _LongName, value, nameof(LongName)); }
        }


        private string _LanguageName = "";
        [JsonProperty]
        public string LanguageName
        {
            get => _LanguageName;
            set { SetProperty(ref _LanguageName, value); }
        }

        private List<BookInfo> _AvailableBooks = new List<BookInfo>();
        [JsonProperty]
        public List<BookInfo> AvailableBooks
        {
            get => _AvailableBooks;
            set { SetProperty(ref _AvailableBooks, value, nameof(AvailableBooks)); }
        }


        private List<string> _NonObservers = new List<string>();
        [JsonProperty]
        public List<string> NonObservers
        {
            get => _NonObservers;
            set { SetProperty(ref _NonObservers, value, nameof(NonObservers)); }
        }


        private ProjectType _Type;
        [JsonProperty]
        public ProjectType Type
        {
            get => _Type;
            set { SetProperty(ref _Type, value, nameof(Type)); }
        }


        private ScrLanguageWrapper _Language;
        [JsonProperty]
        public ScrLanguageWrapper Language
        {
            get => _Language;
            set { SetProperty(ref _Language, value, nameof(Language)); }
        }


    }
}
