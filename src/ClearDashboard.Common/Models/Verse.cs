using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.Common.Models
{
    public class Verse
    {
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
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
        public bool Found { get; set; }
    }
}
