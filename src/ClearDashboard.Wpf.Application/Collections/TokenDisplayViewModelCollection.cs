using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class TokenDisplayViewModelCollection : BindableCollection<TokenDisplayViewModel>
    {
        public NoteViewModelCollection CombinedNotes { get; private set; } = new();

        public string CombinedSurfaceText { get; private set; } = string.Empty;

        public EntityIdCollection EntityIds { get; set; } = new();
            
        public NoteIdCollection NoteIds
        {
            get
            {
                var result = new NoteIdCollection();
                foreach (var ids in Items.Select(i => i.NoteIds))
                {
                    result.AddDistinct(ids);
                }
                return result;
            }
        }

        public TokenDisplayViewModelCollection()
        {
        }

        public TokenDisplayViewModelCollection(TokenDisplayViewModel token) : this()
        {
            Add(token);
        }

        public TokenDisplayViewModelCollection(IEnumerable<TokenDisplayViewModel> tokens) : base(tokens)
        {
        }

        public bool Contains(TokenId tokenId)
        {
            return Items.Any(i => i.Token.TokenId.IdEquals(tokenId.Id));
        }

        public bool Contains(Token token)
        {
            return Contains(token.TokenId);
        }

        public void Remove(TokenId tokenId)
        {
            var existing = Items.FirstOrDefault(i => i.Token.TokenId.IdEquals(tokenId));
            if (existing != null)
            {
                Items.Remove(existing);
            }
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
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(NoteIds)));
        }

        public TokenDisplayViewModelCollection SelectedTokens => new(Items.Where(i => i.IsTokenSelected));
        public TokenDisplayViewModelCollection SelectedTranslations => new(Items.Where(i => i.IsTranslationSelected));
    }
}
