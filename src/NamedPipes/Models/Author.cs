using Newtonsoft.Json;
using ClearDashboard.Common.Models;

namespace ClearDashboard.NamedPipes.Models
{
    public class Author : BindableBase
    {
        private string _Name;
        [JsonProperty]
        public string Name
        {
            get => _Name;
            set { SetProperty(ref _Name, value); }
        }

    }
}
