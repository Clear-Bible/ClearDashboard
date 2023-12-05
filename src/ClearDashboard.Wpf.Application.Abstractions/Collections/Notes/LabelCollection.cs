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

        public void Sort()
        {
            var labels = new List<Label>(this.OrderBy(l => l.Text).ToList());
            Clear();
            foreach (var label in labels)
            {
                Add(label);
            }
        }

        public void Replace(string labelText, Label newLabel, bool addIfNotExists = false)
        {
            var existing = GetMatchingLabel(labelText);
            if (existing != null && existing.LabelId != newLabel.LabelId)
            {
                Items.Insert(Items.IndexOf(existing), newLabel);
                Items.Remove(existing);
            }
            else if (addIfNotExists)
            {
                Items.Add(newLabel);
            }
        }

        public void Replace(Label label, bool addIfNotExists = false)
        {
            if (!string.IsNullOrWhiteSpace(label.Text))
            {
                Replace(label.Text, label, addIfNotExists);
            }
        }

        public void RemoveIfExists(Label label)
        {
            if (label != null)
            {
                var existing = Items.FirstOrDefault(l => l.Text == label.Text);
                if (existing != null)
                {
                    Items.Remove(existing);
                }
            }
        }
    }
}
