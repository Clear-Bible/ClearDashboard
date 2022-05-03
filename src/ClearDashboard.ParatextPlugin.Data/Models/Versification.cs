using MvvmHelpers;

namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public  class Versification : ObservableObject
    {
        private int _type;
        public int Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private bool _isCustomized;
        public bool IsCustomized
        {
            get => _isCustomized;
            set => SetProperty(ref _isCustomized, value);
        }
    }
}
