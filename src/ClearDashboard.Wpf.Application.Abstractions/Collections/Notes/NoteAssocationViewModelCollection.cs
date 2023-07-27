using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Collections.Generic;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class NoteAssociationViewModelCollection : BindableCollection<NoteAssociationViewModel>
    {
        public NoteAssociationViewModelCollection()
        {
        }

        public NoteAssociationViewModelCollection(IEnumerable<NoteAssociationViewModel> noteAssociations) : base(noteAssociations)
        {
        }
    }
}
