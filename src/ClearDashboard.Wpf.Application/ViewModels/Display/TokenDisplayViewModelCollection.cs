using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class TokenDisplayViewModelCollection : ObservableCollection<TokenDisplayViewModel>, INotifyPropertyChanged
    {
        public NoteViewModelCollection CombinedNotes { get; private set; } = new();

        public string CombinedSurfaceText { get; private set; } = string.Empty;

        public EntityIdCollection EntityIds { get; private set; } = new(); 

        public TokenDisplayViewModelCollection()
        {
        }

        public TokenDisplayViewModelCollection(TokenDisplayViewModel token) : this()
        {
            Add(token);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CombinedNotes = new NoteViewModelCollection();
            foreach (var token in Items)
            {
                foreach (var note in token.Notes)
                {
                    if (!CombinedNotes.Contains(note))
                    {
                        CombinedNotes.Add(note);
                    }
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedNotes)));

            CombinedSurfaceText = string.Join(", ", Items.Select(t => t.SurfaceText));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedSurfaceText)));

            EntityIds = new EntityIdCollection(Items.Select(t => t.Token.TokenId));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(EntityIds)));
        }
    }
}
