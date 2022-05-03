namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public class ScrLanguageWrapper : BindableBase
    {

        private string _fontFamily = "Segoe UI";
        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value, nameof(FontFamily));
        }

        private float _size = 13;
        public float Size
        {
            get => _size;
            set => SetProperty(ref _size, value, nameof(Size));
        }

        private bool _isRtol;
        public bool IsRtol
        {
            get => _isRtol;
            set => SetProperty(ref _isRtol, value, nameof(IsRtol));
        }

        private ScrLanguage _language;
        public ScrLanguage Language
        {
            get => _language;
            set => SetProperty(ref _language, value, nameof(Language));
        }

    }
}
