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
            var existing = Items.FirstOrDefault(i => i.Text != null && i.Text.Equals(label.Text));
            if (existing == null)
            {
                Add(label);
            }
        }

        public Label? GetMatchingLabel(string? text)
        {
            return ! string.IsNullOrWhiteSpace(text) ? Items.FirstOrDefault(i => i.Text != null && i.Text == text) : null;
        }

        public bool ContainsMatchingLabel(string? text)
        {
            return GetMatchingLabel(text) != null;
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
