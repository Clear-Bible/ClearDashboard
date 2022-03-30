using MvvmHelpers;
using System.ComponentModel;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Language : ObservableObject
    {
        private string _FontFamily;
        public string FontFamily
        {
            get => _FontFamily;
            set { SetProperty(ref _FontFamily, value); }
        }

        private double _Size;
        public double Size
        {
            get => _Size;
            set { SetProperty(ref _Size, value); }
        }

        private string _language;
        [DisplayName("Language")]
        public string language
        {
            get => _language;
            set { SetProperty(ref _language, value); }
        }

        private string _Features;
        public string Features
        {
            get => _Features;
            set { SetProperty(ref _Features, value); }
        }

        private string _Id;
        public string Id
        {
            get => _Id;
            set { SetProperty(ref _Id, value); }
        }

        private bool _IsRtoL;
        public bool IsRtoL
        {
            get => _IsRtoL;
            set { SetProperty(ref _IsRtoL, value); }
        }
    }
}
