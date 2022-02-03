using Newtonsoft.Json;
using ClearDashboard.Common.Models;

namespace ClearDashboard.NamedPipes.Models
{
    public class Anchor : BindableBase
    {
        private VerseRefStart _VerseRefStart;
        [JsonProperty]
        public VerseRefStart VerseRefStart
        {
            get => _VerseRefStart;
            set { SetProperty(ref _VerseRefStart, value); }
        }

        private VerseRefEnd _VerseRefEnd;
        [JsonProperty]
        public VerseRefEnd VerseRefEnd
        {
            get => _VerseRefEnd;
            set { SetProperty(ref _VerseRefEnd, value); }
        }

        private string _SelectedText;
        [JsonProperty]
        public string SelectedText
        {
            get => _SelectedText;
            set { SetProperty(ref _SelectedText, value); }
        }

        private int _Offset;
        [JsonProperty]
        public int Offset
        {
            get => _Offset;
            set { SetProperty(ref _Offset, value); }
        }

        private string _BeforeContext;
        [JsonProperty]
        public string BeforeContext
        {
            get => _BeforeContext;
            set { SetProperty(ref _BeforeContext, value); }
        }

        private string _AfterContext;
        [JsonProperty]
        public string AfterContext
        {
            get => _AfterContext;
            set { SetProperty(ref _AfterContext, value); }
        }
    }
}