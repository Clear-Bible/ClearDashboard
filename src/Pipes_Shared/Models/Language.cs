using Newtonsoft.Json;
using System.ComponentModel;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Language : ObservableObject
    {
        private string _FontFamily;
        [JsonProperty]
        public string FontFamily
        {
            get => _FontFamily;
            set { SetProperty(ref _FontFamily, value); }
        }

        private double _Size;
        [JsonProperty]
        public double Size
        {
            get => _Size;
            set { SetProperty(ref _Size, value); }
        }

        private string _language;
        [JsonProperty]
        [DisplayName("Language")]
        public string language
        {
            get => _language;
            set { SetProperty(ref _language, value); }
        }

        private string _Features;
        [JsonProperty]
        public string Features
        {
            get => _Features;
            set { SetProperty(ref _Features, value); }
        }

        private string _Id;
        [JsonProperty]
        public string Id
        {
            get => _Id;
            set { SetProperty(ref _Id, value); }
        }

        private bool _IsRtoL;
        [JsonProperty]
        public bool IsRtoL
        {
            get => _IsRtoL;
            set { SetProperty(ref _IsRtoL, value); }
        }
    }
}
