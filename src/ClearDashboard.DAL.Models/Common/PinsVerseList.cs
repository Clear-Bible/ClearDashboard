using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class PinsVerseList
    {
        public string? BBBCCCVVV { get; set; }
        public string? VerseIdShort { get; set; }
        public string? VerseText { get; set; }
        public bool Found { get; set; }
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
    }
}
