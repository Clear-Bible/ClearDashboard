using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public  class Versification : ObservableObject
    {
        private int _Type;
        public int Type
        {
            get => _Type;
            set { SetProperty(ref _Type, value); }
        }

        private bool _IsCustomized;
        public bool IsCustomized
        {
            get => _IsCustomized;
            set { SetProperty(ref _IsCustomized, value); }
        }
    }
}
