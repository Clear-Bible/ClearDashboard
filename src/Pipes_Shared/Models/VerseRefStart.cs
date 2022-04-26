using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class VerseRefStart : ObservableObject
    {
        private string _bookCode;
        public string BookCode
        {
            get => _bookCode;
            set => SetProperty(ref _bookCode, value);
        }

        private int _bookNum;
        public int BookNum
        {
            get => _bookNum;
            set => SetProperty(ref _bookNum, value);
        }

        private int _chapterNum;
        public int ChapterNum
        {
            get => _chapterNum;
            set => SetProperty(ref _chapterNum, value);
        }

        private int _verseNum;
        public int VerseNum
        {
            get => _verseNum;
            set => SetProperty(ref _verseNum, value);
        }

        private int _bbbcccvvv;
        public int Bbbcccvvv
        {
            get => _bbbcccvvv;
            set => SetProperty(ref _bbbcccvvv, value);
        }

        private Versification _versification;
        public Versification Versification
        {
            get => _versification;
            set => SetProperty(ref _versification, value);
        }

        private bool _representsMultipleVerses;
        public bool RepresentsMultipleVerses
        {
            get => _representsMultipleVerses;
            set => SetProperty(ref _representsMultipleVerses, value);
        }

        private List<object> _allVerses;
        public List<object> AllVerses
        {
            get => _allVerses;
            set => SetProperty(ref _allVerses, value);
        }
    }
}