using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ClearDashboard.Wpf.Application.Models
{
    public class TextCollectionList
    {
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
    }
}
