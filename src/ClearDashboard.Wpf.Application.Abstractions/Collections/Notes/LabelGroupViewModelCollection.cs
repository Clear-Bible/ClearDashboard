using Caliburn.Micro;
using System.Collections.Generic;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class LabelGroupViewModelCollection : BindableCollection<LabelGroupViewModel>
    {
        public LabelGroupViewModelCollection()
        {
        }

        public LabelGroupViewModelCollection(IEnumerable<LabelGroupViewModel> labelGroups) : base(labelGroups)
        {
        }
    }
}
