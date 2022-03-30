using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Span : ObservableObject
    {
        private string _Text;
        public string Text
        {
            get => _Text;
            set { SetProperty(ref _Text, value); }
        }

        private int _Style;
        public int Style
        {
            get => _Style;
            set { SetProperty(ref _Style, value); }
        }

        private object _Language;
        public object Language
        {
            get => _Language;
            set { SetProperty(ref _Language, value); }
        }
    }
}