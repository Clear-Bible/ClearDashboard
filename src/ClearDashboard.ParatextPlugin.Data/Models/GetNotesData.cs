using MvvmHelpers;

namespace ClearDashboard.ParatextPlugin.Data.Models
{
    public class GetNotesData : ObservableObject
    {
        private int _bookId;
        public int BookId
        {
            get => _bookId;
            set
            {
                _bookId = value;
                OnPropertyChanged();
            }
        }

        private int _chapterId;
        public int ChapterId
        {
            get => _chapterId;
            set
            {
                _chapterId = value;
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
