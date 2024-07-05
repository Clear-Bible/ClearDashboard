using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class TokenCharacterViewModel : PropertyChangedBase
    {
        private char _character;
        private int _index;
        private bool _isSelected;

        public char Character
        {
            get => _character;
            set => Set(ref _character, value);
        }

        public int Index
        {
            get => _index;
            set => Set(ref _index, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
    }
}
