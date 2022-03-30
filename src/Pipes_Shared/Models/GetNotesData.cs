using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class GetNotesData : ObservableObject
    {
        private int _bookID;
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
