namespace ParaTextPlugin.Data.Models
{
    public class ScrLanguage : BindableBase
    {

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value, nameof(DisplayName));
        }


        private string _languageTag;
        public string LanguageTag
        {
            get => _languageTag;
            set => SetProperty(ref _languageTag, value, nameof(LanguageTag));
        }

    }
}
