using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.Wpf.Models
{
    public class TextCollectionList
    {
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
    }
}
