using Pipes_Shared.Models;
using Newtonsoft.Json;

namespace Pipes_Shared.Models
{
    public class ScrLanguageWrapper : BindableBase
    {

        private string _FontFamily;
        [JsonProperty]
        public string FontFamily
        {
            get => _FontFamily;
            set { SetProperty(ref _FontFamily, value, nameof(FontFamily)); }
        }

        private bool _IsRtol;
        [JsonProperty]
        public bool IsRtol
        {
            get => _IsRtol;
            set { SetProperty(ref _IsRtol, value, nameof(IsRtol)); }
        }

        private ScrLanguage _language;
        [JsonProperty]
        public ScrLanguage language
        {
            get => _language;
            set { SetProperty(ref _language, value, nameof(language)); }
        }

    }
}
