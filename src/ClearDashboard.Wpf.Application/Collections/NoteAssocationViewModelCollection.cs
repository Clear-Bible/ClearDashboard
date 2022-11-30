using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class NoteAssociationViewModelCollection : ObservableCollection<NoteAssociationViewModel>
    {
        public NoteAssociationViewModelCollection()
        {
        }

        public NoteAssociationViewModelCollection(IEnumerable<NoteAssociationViewModel> noteAssociations) : base(noteAssociations)
        {
        }
    }
}
