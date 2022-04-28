using MvvmHelpers;

namespace ParaTextPlugin.Data.Models
{
    public class Span : ObservableObject
    {
        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private int _style;
        public int Style
        {
            get => _style;
            set => SetProperty(ref _style, value);
        }

        private object _language;
        public object Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }
    }
}