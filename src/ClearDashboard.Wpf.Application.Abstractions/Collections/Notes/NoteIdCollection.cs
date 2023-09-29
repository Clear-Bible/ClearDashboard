using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using System.Collections.Generic;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class NoteIdCollection : BindableCollection<NoteId>
    {
        public NoteIdCollection()
        {

        }

        public NoteIdCollection(IEnumerable<NoteId> ids) : base(ids)
        {

        }

        public void AddDistinct(NoteId id)
        {
            var existing = Items.FirstOrDefault(i => i.IdEquals(id));
            if (existing == null)
            {
                Items.Add(id);
            }
        }

        public void AddDistinct(IEnumerable<NoteId> ids)
        {
            foreach (var id in ids)
            {
                AddDistinct(id);
            }
        }

        public void RemoveIfExists(NoteId id)
        {
            var existing = Items.FirstOrDefault(i => i.IdEquals(id));
            if (existing != null)
            {
                Items.Remove(existing);
            }
        }
    }
}
