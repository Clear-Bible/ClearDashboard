using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using ClearDashboard.DAL.Alignment.Notes;
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

        public Label? GetLabel(string? text)
        {
            return this.Select(labelGroup => labelGroup.Labels.FirstOrDefault(l => l.Text == text)).FirstOrDefault(label => label != null);
        }
    }
}
