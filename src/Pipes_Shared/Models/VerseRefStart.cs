using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.Pipes_Shared.Models
{
    public class VerseRefStart : ObservableObject
    {
        private string _BookCode;
        public string BookCode
        {
            get => _BookCode;
            set { SetProperty(ref _BookCode, value); }
        }

        private int _BookNum;
        public int BookNum
        {
            get => _BookNum;
            set { SetProperty(ref _BookNum, value); }
        }

        private int _ChapterNum;
        public int ChapterNum
        {
            get => _ChapterNum;
            set { SetProperty(ref _ChapterNum, value); }
        }

        private int _VerseNum;
        public int VerseNum
        {
            get => _VerseNum;
            set { SetProperty(ref _VerseNum, value); }
        }

        private int _BBBCCCVVV;
        public int BBBCCCVVV
        {
            get => _BBBCCCVVV;
            set { SetProperty(ref _BBBCCCVVV, value); }
        }

        private Versification _Versification;
        public Versification Versification
        {
            get => _Versification;
            set { SetProperty(ref _Versification, value); }
        }

        private bool _RepresentsMultipleVerses;
        public bool RepresentsMultipleVerses
        {
            get => _RepresentsMultipleVerses;
            set { SetProperty(ref _RepresentsMultipleVerses, value); }
        }

        private List<object> _AllVerses;
        public List<object> AllVerses
        {
            get => _AllVerses;
            set { SetProperty(ref _AllVerses, value); }
        }
    }
}