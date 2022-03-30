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
        public string ID
        {
            get => _ID;
            set { SetProperty(ref _ID, value); }
        }

        private string _ShortName = "";
        public string ShortName
        {
            get => _ShortName;
            set { SetProperty(ref _ShortName, value); }
        }


        private string _LongName = "";
        public string LongName
        {
            get => _LongName;
            set { SetProperty(ref _LongName, value, nameof(LongName)); }
        }


        private string _LanguageName = "";
        public string LanguageName
        {
            get => _LanguageName;
            set { SetProperty(ref _LanguageName, value); }
        }

        private List<BookInfo> _AvailableBooks = new List<BookInfo>();
        public List<BookInfo> AvailableBooks
        {
            get => _AvailableBooks;
            set { SetProperty(ref _AvailableBooks, value, nameof(AvailableBooks)); }
        }


        private List<string> _NonObservers = new List<string>();
        public List<string> NonObservers
        {
            get => _NonObservers;
            set { SetProperty(ref _NonObservers, value, nameof(NonObservers)); }
        }


        private ProjectType _Type;
        public ProjectType Type
        {
            get => _Type;
            set { SetProperty(ref _Type, value, nameof(Type)); }
        }


        private ScrLanguageWrapper _Language;
        public ScrLanguageWrapper Language
        {
            get => _Language;
            set { SetProperty(ref _Language, value, nameof(Language)); }
        }


    }
}
