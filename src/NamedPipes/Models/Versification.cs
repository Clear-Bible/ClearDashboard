using Newtonsoft.Json;

namespace ClearDashboard.NamedPipes.Models
{
    public  class Versification : BindableBase
    {
        private int _Type;
        [JsonProperty]
        public int Type
        {
            get => _Type;
            set { SetProperty(ref _Type, value); }
        }

        private bool _IsCustomized;
        [JsonProperty]
        public bool IsCustomized
        {
            get => _IsCustomized;
            set { SetProperty(ref _IsCustomized, value); }
        }
    }
}
