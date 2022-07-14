using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Documents;

namespace Models
{
    public class TextCollectionList
    {
        public ObservableCollection<Inline> Inlines { get; set; } = new ObservableCollection<Inline>();
    }
}
