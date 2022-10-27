using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
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
