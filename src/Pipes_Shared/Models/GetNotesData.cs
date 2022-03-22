using Newtonsoft.Json;
using MvvmHelpers;

namespace ClearDashboard.NamedPipes.Models
{
    public class GetNotesData : ObservableObject
    {
        private int _bookID;
        [JsonProperty]
        public int BookID
        {
            get => _bookID;
            set
            {
                _bookID = value;
                OnPropertyChanged();
            }
        }

        private int _chapterID;
        [JsonProperty]
        public int ChapterID
        {
            get => _chapterID;
            set
            {
                _chapterID = value;
                OnPropertyChanged();
            }
        }

        private bool _includeResolved;
        [JsonProperty]
        public bool IncludeResolved
        {
            get => _includeResolved;
            set
            {
                _includeResolved = value;
                OnPropertyChanged();
            }
        }

    }
}
