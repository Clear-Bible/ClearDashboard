using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class NoteViewModelCollection : BindableCollection<NoteViewModel>
    {
        public NoteViewModelCollection()
        {

        }

        public NoteViewModelCollection(IEnumerable<NoteViewModel> notes) : base(notes)
        {

        }
    }
}
