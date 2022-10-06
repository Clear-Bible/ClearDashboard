using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class NoteCollection : ObservableCollection<Note>
    {
        public NoteCollection()
        {
            
        }

        public NoteCollection(IEnumerable<Note> notes) : base(notes)
        {
            
        }
    }
}
