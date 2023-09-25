using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    public class LabelCollection : BindableCollection<Label>
    {
        public LabelCollection()
        {
        }

        public LabelCollection(IEnumerable<Label> labels) : base(labels)
        {
        }

        public void AddDistinct(Label label)
        {
            if (!Contains(label))
            {
                Add(label);
            }
        }

        public void Insert(Label label)
        {
            throw new NotImplementedException();
        }

        public void RemoveIfExists(Label label)
        {
            if (label != null)
            {
                var existing = Items.FirstOrDefault(i => i.LabelId != null && i.LabelId.Equals(label.LabelId));
                if (existing != null)
                {
                    Items.Remove(existing);
                }
            }
        }
    }
}
