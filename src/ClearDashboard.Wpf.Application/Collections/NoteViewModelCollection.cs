using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class NoteViewModelCollection : ObservableCollection<NoteViewModel>
    {
        public NoteViewModelCollection()
        {

        }

        public NoteViewModelCollection(IEnumerable<NoteViewModel> notes) : base(notes)
        {

        }
    }
}
