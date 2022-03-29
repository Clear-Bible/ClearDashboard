using Pipes_Shared.Models;
using Newtonsoft.Json;

namespace Pipes_Shared.Models
{
    public class BookInfo : BindableBase
    {
        private string _Code;
        [JsonProperty]
        public string Code
        {
            get => _Code;
            set { SetProperty(ref _Code, value, nameof(Code)); }
        }


        private bool _inProjectScope;
        [JsonProperty]
        public bool InProjectScope
        {
            get => _inProjectScope;
            set { SetProperty(ref _inProjectScope, value, nameof(InProjectScope)); }
        }


        private int _Number;
        [JsonProperty]
        public int Number
        {
            get => _Number;
            set { SetProperty(ref _Number, value, nameof(Number)); }
        }
    }
}
