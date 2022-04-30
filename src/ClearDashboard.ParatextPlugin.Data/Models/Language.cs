using System.ComponentModel;
using MvvmHelpers;

namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public class SelectedLanguage : ObservableObject
    {
        private string _fontFamily;
        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        private double _size;
        public double Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private string _language;
        [DisplayName("Language")]
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        private string _features;
        public string Features
        {
            get => _features;
            set => SetProperty(ref _features, value);
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _isRtoL;
        public bool IsRtoL
        {
            get => _isRtoL;
            set => SetProperty(ref _isRtoL, value);
        }
    }
}
