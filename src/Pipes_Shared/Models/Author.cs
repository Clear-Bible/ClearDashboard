using Newtonsoft.Json;
using MvvmHelpers;

namespace ClearDashboard.NamedPipes.Models
{
    public class Author : ObservableObject
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
