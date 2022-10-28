using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class LabelCollection : ObservableCollection<Label>
    {
        public LabelCollection()
        {
        }

        public LabelCollection(IEnumerable<Label> labels) : base(labels)
        {
        }
    }
}
