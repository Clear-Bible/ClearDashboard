using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.Common.Models
{
    public class Verse
    {
        public string VerseID { get; set; } = string.Empty;
        public string VerseBBCCCVVV { get; set; } = String.Empty;
        public string VerseText { get; set; }
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
        public bool Found { get; set; }
    }
}
