using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class NoteCollection : BindableCollection<Note>
    {
        public NoteCollection()
        {
        }

        public NoteCollection(IEnumerable<Note> notes) : base(notes)
        {
        }
    }
}
