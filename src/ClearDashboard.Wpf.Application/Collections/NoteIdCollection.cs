using ClearBible.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class NoteIdCollection : BindableCollection<NoteId>
    {
        public NoteIdCollection()
        {

        }

        public NoteIdCollection(IEnumerable<NoteId> ids) : base(ids)
        {

        }

        public void AddDistinct(IEnumerable<NoteId> ids)
        {
            foreach (var id in ids)
            {
                if (!Items.Contains(id))
                {
                    Add(id);
                }
            }
        }
    }
}
