using Newtonsoft.Json;

namespace ClearDashboard.NamedPipes.Models
{
    public class Span : BindableBase
    {
        private string _Text;
        [JsonProperty]
        public string Text
        {
            get => _Text;
            set { SetProperty(ref _Text, value); }
        }

        private int _Style;
        [JsonProperty]
        public int Style
        {
            get => _Style;
            set { SetProperty(ref _Style, value); }
        }

        private object _Language;
        [JsonProperty]
        public object Language
        {
            get => _Language;
            set { SetProperty(ref _Language, value); }
        }
    }
}