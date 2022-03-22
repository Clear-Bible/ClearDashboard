using Newtonsoft.Json;
using System.Collections.Generic;
using MvvmHelpers;

namespace ClearDashboard.NamedPipes.Models
{
    public class VerseRefStart : ObservableObject
    {
        private string _BookCode;
        [JsonProperty]
        public string BookCode
        {
            get => _BookCode;
            set { SetProperty(ref _BookCode, value); }
        }

        private int _BookNum;
        [JsonProperty]
        public int BookNum
        {
            get => _BookNum;
            set { SetProperty(ref _BookNum, value); }
        }

        private int _ChapterNum;
        [JsonProperty]
        public int ChapterNum
        {
            get => _ChapterNum;
            set { SetProperty(ref _ChapterNum, value); }
        }

        private int _VerseNum;
        [JsonProperty]
        public int VerseNum
        {
            get => _VerseNum;
            set { SetProperty(ref _VerseNum, value); }
        }

        private int _BBBCCCVVV;
        [JsonProperty]
        public int BBBCCCVVV
        {
            get => _BBBCCCVVV;
            set { SetProperty(ref _BBBCCCVVV, value); }
        }

        private Versification _Versification;
        [JsonProperty]
        public Versification Versification
        {
            get => _Versification;
            set { SetProperty(ref _Versification, value); }
        }

        private bool _RepresentsMultipleVerses;
        [JsonProperty]
        public bool RepresentsMultipleVerses
        {
            get => _RepresentsMultipleVerses;
            set { SetProperty(ref _RepresentsMultipleVerses, value); }
        }

        private List<object> _AllVerses;
        [JsonProperty]
        public List<object> AllVerses
        {
            get => _AllVerses;
            set { SetProperty(ref _AllVerses, value); }
        }
    }
}