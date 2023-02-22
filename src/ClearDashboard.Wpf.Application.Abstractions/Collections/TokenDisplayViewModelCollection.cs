﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
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

        public IEnumerable<TokenId> TokenIds => Items.Select(t => t.Token.TokenId);

        public TokenCollection TokenCollection => new(Items.Select(t => t.Token));

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

        private IEnumerable<TokenDisplayViewModel> SelectedTokens => Items.Where(i => i.IsTokenSelected);
        private IEnumerable<TokenDisplayViewModel> SelectedTranslations => Items.Where(i => i.IsTranslationSelected);
        
        private int SelectedTokenVersesCount => SelectedTokens.Select(t => t.VerseDisplay).Distinct(ReferenceEqualityComparer.Instance).Count();
        private int SelectedTokenCompositeTokenCount => SelectedTokens.Select(t => t.CompositeToken).Distinct(ReferenceEqualityComparer.Instance).Count();

        public IEnumerable<TokenDisplayViewModel> MatchingTokens(IEnumerable<IId> entityIds)
        {
            return Items.Where(t => entityIds.Contains(t.Token.TokenId, new IIdEqualityComparer()));
        }

        private IEnumerable<TokenDisplayViewModel> MatchingTokens(Func<TokenDisplayViewModel, bool> conditional)
        {
            return Items.Where(conditional);
        }

        private IEnumerable<TokenDisplayViewModel> NonMatchingTokens(IEnumerable<IId> entityIds)
        {
            return Items.Where(t => ! entityIds.Contains(t.Token.TokenId, new IIdEqualityComparer()));
        }

        public void MatchingTokenAction(IEnumerable<IId> entityIds, Action<TokenDisplayViewModel> action)
        {
            foreach (var token in MatchingTokens(entityIds))
            {
                action(token);
            }
        }

        public void MatchingTokenAction(Func<TokenDisplayViewModel, bool> conditional, Action<TokenDisplayViewModel> action)
        {
            foreach (var token in MatchingTokens(conditional))
            {
                action(token);
            }
        }

        public void NonMatchingTokenAction(IEnumerable<IId> entityIds, Action<TokenDisplayViewModel> action)
        {
            foreach (var token in NonMatchingTokens(entityIds))
            {
                action(token);
            }
        }

        public bool CanJoinTokens => SelectedTokens.Count() > 1 && SelectedTokens.All(t => ! t.IsCompositeTokenMember) && SelectedTokenVersesCount == 1 && !SelectedTranslations.Any();
        public bool CanUnjoinToken => SelectedTokens.All(t => t.IsCompositeTokenMember) && SelectedTokenCompositeTokenCount == 1;

        public bool CanDeleteAlignment => SelectedTokens.Any(t => t.IsHighlighted);
    }
}
