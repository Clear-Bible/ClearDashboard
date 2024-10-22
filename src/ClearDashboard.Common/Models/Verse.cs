﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.Common.Models
{
    public class Verse
    {
        public string BookStr
        {
            get
            {
                var book = _VerseBBCCCVVV.Substring(0, 2);
                return book;
            }
        }

        public int BookNum
        {
            get
            {
                var book = _VerseBBCCCVVV.Substring(0, 2);
                return Convert.ToInt32(book);
            }
        }

        public string ChapterStr
        {
            get
            {
                var chap = _VerseBBCCCVVV.Substring(2, 3);
                return chap;
            }
        }

        public int ChapterNum
        {
            get
            {
                var chap = _VerseBBCCCVVV.Substring(2, 3);
                return Convert.ToInt32(chap);
            }
        }

        public string VerseStr
        {
            get
            {
                var verse = _VerseBBCCCVVV.Substring(5, 3);
                return verse;
            }
        }

        public int VerseNum
        {
            get
            {
                var verse = _VerseBBCCCVVV.Substring(5, 3);
                return Convert.ToInt32(verse);
            }
        }

        public string VerseID { get; set; } = string.Empty;

        private string _VerseBBCCCVVV = string.Empty;
        public string VerseBBCCCVVV
        {
            get
            {
                return _VerseBBCCCVVV;
            }
            set
            {
                _VerseBBCCCVVV = value;
                if (_VerseBBCCCVVV.Length < 8)
                {
                    _VerseBBCCCVVV = _VerseBBCCCVVV.PadLeft(8, '0');
                }
            }
        }

        public string VerseText { get; set; }
        public bool Found { get; set; }
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
     
    }
}
