using MvvmHelpers;

namespace ParaTextPlugin.Data.Models
{
    public class Anchor : ObservableObject
    {
        private VerseRefStart _verseRefStart;
        public VerseRefStart VerseRefStart
        {
            get => _verseRefStart;
            set => SetProperty(ref _verseRefStart, value);
        }

        private VerseRefEnd _verseRefEnd;
        public VerseRefEnd VerseRefEnd
        {
            get => _verseRefEnd;
            set => SetProperty(ref _verseRefEnd, value);
        }

        private string _selectedText;
        public string SelectedText
        {
            get => _selectedText;
            set => SetProperty(ref _selectedText, value);
        }

        private int _offset;
        public int Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }

        private string _beforeContext;
        public string BeforeContext
        {
            get => _beforeContext;
            set => SetProperty(ref _beforeContext, value);
        }

        private string _afterContext;
        public string AfterContext
        {
            get => _afterContext;
            set => SetProperty(ref _afterContext, value);
        }
    }
}