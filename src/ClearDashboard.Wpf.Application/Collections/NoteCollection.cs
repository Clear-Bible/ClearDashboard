using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.Collections
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
