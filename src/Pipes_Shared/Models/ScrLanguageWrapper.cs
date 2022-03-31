namespace Pipes_Shared.Models
{
    public class ScrLanguageWrapper : BindableBase
    {

        private string _FontFamily = "Segoe UI";
        public string FontFamily
        {
            get => _FontFamily;
            set { SetProperty(ref _FontFamily, value, nameof(FontFamily)); }
        }

        private float _Size = 13;
        public float Size
        {
            get => _Size;
            set { SetProperty(ref _Size, value, nameof(Size)); }
        }

        private bool _IsRtol;
        public bool IsRtol
        {
            get => _IsRtol;
            set { SetProperty(ref _IsRtol, value, nameof(IsRtol)); }
        }

        private ScrLanguage _language;
        public ScrLanguage language
        {
            get => _language;
            set { SetProperty(ref _language, value, nameof(language)); }
        }

    }
}
