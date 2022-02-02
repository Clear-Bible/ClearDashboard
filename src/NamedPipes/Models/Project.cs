using ClearDashboard.Common.Models;
using Newtonsoft.Json;

namespace NamedPipes.Models
{
    public class Project : BindableBase
    {
        private string _ID;
        [JsonProperty]
        public string ID
        {
            get => _ID;
            set { SetProperty(ref _ID, value); }
        }

        private string _ShortName;
        [JsonProperty]
        public string ShortName
        {
            get => _ShortName;
            set { SetProperty(ref _ShortName, value); }
        }

        private string _LanguageName;
        [JsonProperty]
        public string LanguageName
        {
            get => _LanguageName;
            set { SetProperty(ref _LanguageName, value); }
        }

        private string _Type;
        [JsonProperty]
        public string Type
        {
            get => _Type;
            set { SetProperty(ref _Type, value); }
        }

        private BaseProject _BaseProject;
        [JsonProperty]
        public BaseProject BaseProject
        {
            get => _BaseProject;
            set { SetProperty(ref _BaseProject, value); }
        }
        

    }
}
