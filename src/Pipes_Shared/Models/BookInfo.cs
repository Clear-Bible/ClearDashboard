namespace Pipes_Shared.Models
{
    public class BookInfo : BindableBase
    {
        private string _Code;
        public string Code
        {
            get => _Code;
            set { SetProperty(ref _Code, value, nameof(Code)); }
        }


        private bool _inProjectScope;
        public bool InProjectScope
        {
            get => _inProjectScope;
            set { SetProperty(ref _inProjectScope, value, nameof(InProjectScope)); }
        }


        private int _Number;
        public int Number
        {
            get => _Number;
            set { SetProperty(ref _Number, value, nameof(Number)); }
        }
    }
}
