using System.Text.Json.Serialization;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class Anchor : ObservableObject
    {
        private VerseRefStart _VerseRefStart;
        public VerseRefStart VerseRefStart
        {
            get => _VerseRefStart;
            set { SetProperty(ref _VerseRefStart, value); }
        }

        private VerseRefEnd _VerseRefEnd;
        public VerseRefEnd VerseRefEnd
        {
            get => _VerseRefEnd;
            set { SetProperty(ref _VerseRefEnd, value); }
        }

        private string _SelectedText;
        public string SelectedText
        {
            get => _SelectedText;
            set { SetProperty(ref _SelectedText, value); }
        }

        private int _Offset;
        public int Offset
        {
            get => _Offset;
            set { SetProperty(ref _Offset, value); }
        }

        private string _BeforeContext;
        public string BeforeContext
        {
            get => _BeforeContext;
            set { SetProperty(ref _BeforeContext, value); }
        }

        private string _AfterContext;
        public string AfterContext
        {
            get => _AfterContext;
            set { SetProperty(ref _AfterContext, value); }
        }
    }
}