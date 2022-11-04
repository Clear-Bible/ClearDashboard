using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClearDashboard.Wpf.Application.ViewModels.Display;

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
