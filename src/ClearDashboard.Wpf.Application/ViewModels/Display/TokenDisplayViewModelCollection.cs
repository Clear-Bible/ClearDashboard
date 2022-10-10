using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ClearBible.Engine.Utils;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class TokenDisplayViewModelCollection : ObservableCollection<TokenDisplayViewModel>, INotifyPropertyChanged
    {
        public NoteCollection CombinedNotes { get; private set; }

        public string CombinedSurfaceText { get; private set; }

        public EntityIdCollection EntityIds { get; private set; }

        public TokenDisplayViewModelCollection()
        {
            CollectionChanged += OnCollectionChanged;
        }

        public TokenDisplayViewModelCollection(TokenDisplayViewModel token) : this()
        {
            Add(token);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CombinedNotes = new NoteCollection();
            foreach (var token in Items)
            {
                CombinedNotes.AddRange(token.Notes);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedNotes)));

            CombinedSurfaceText = string.Join(", ", Items.Select(t => t.SurfaceText));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedSurfaceText)));

            EntityIds = new EntityIdCollection(Items.Select(t => t.Token.TokenId));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(EntityIds)));
        }
    }
}
