namespace ParaTextPlugin.Data.Models
{
    public class BookInfo : BindableBase
    {
        private string _code;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value, nameof(Code));
        }


        private bool _inProjectScope;
        public bool InProjectScope
        {
            get => _inProjectScope;
            set => SetProperty(ref _inProjectScope, value, nameof(InProjectScope));
        }


        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value, nameof(Number));
        }
    }
}
