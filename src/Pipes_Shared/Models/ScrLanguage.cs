using Pipes_Shared.Models;
using Newtonsoft.Json;

namespace Pipes_Shared.Models
{
    public class ScrLanguage : BindableBase
    {

        private string _DisplayName;
        [JsonProperty]
        public string DisplayName
        {
            get => _DisplayName;
            set { SetProperty(ref _DisplayName, value, nameof(DisplayName)); }
        }


        private string _LanguageTag;
        [JsonProperty]
        public string LanguageTag
        {
            get => _LanguageTag;
            set { SetProperty(ref _LanguageTag, value, nameof(LanguageTag)); }
        }

    }
}
