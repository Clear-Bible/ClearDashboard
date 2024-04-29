using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections.Notes;
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

                var tokenNoteIds = Items
                    .Where(i => i.IsTokenSelected)
                    .Select(i => i.TokenNoteIds);
                var translationNoteIds = Items
                    .Where(i => i.IsTranslationSelected)
                    .Select(i => i.TranslationNoteIds);

                foreach (var ids in tokenNoteIds.Union(translationNoteIds))
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
            RecalculateEntityIds();
        }

        public IEnumerable<TokenId> TokenIds => Items.Select(t => t.Token.TokenId);
        public IEnumerable<TranslationId> TranslationIds => Items
            .Where(t => t.Translation?.TranslationId != null)
            .Select(t => t.Translation!.TranslationId!);

        public TokenCollection TokenCollection => new(Items.Select(t => t.Token));

        public void AddDistinct(TokenDisplayViewModel tokenDisplay)
        {
            if (!Contains(tokenDisplay.Token.TokenId))
            {
                Add(tokenDisplay);
            }
            RecalculateEntityIds();
        }

        public void AddRangeDistinct(IEnumerable<TokenDisplayViewModel> tokenDisplays)
        {
            AddRange(tokenDisplays.Where(t => !Contains(t.Token.TokenId)));
            RecalculateEntityIds();
        }

        /// <summary>
        /// Gets a collection containing a sequence of tokens between a beginning token and ending token, inclusive.
        /// </summary>
        /// <param name="beginToken">The beginning <see cref="TokenDisplayViewModel"/>.</param>
        /// <param name="endToken">The ending <see cref="TokenDisplayViewModel"/>.</param>
        /// <remarks>
        /// If either <paramref name="beginToken"/> and/or <paramref name="endToken"/> are not found in the collection, then an
        /// empty <see cref="TokenDisplayViewModelCollection"/> is returned.
        /// </remarks>
        /// <returns>
        /// A new <see cref="TokenDisplayViewModelCollection"/> containing all the <see cref="TokenDisplayViewModel"/>
        /// instances between the beginning token and ending token, inclusive.
        /// </returns>
        public TokenDisplayViewModelCollection GetRange(TokenDisplayViewModel beginToken, TokenDisplayViewModel endToken)
        {
            var beginTokenIndex = Math.Min(IndexOf(beginToken), IndexOf(endToken));
            var endTokenIndex = Math.Max(IndexOf(beginToken), IndexOf(endToken));

            return beginTokenIndex != -1 && endTokenIndex != -1
                ? new TokenDisplayViewModelCollection(this.Skip(beginTokenIndex).Take(endTokenIndex - beginTokenIndex + 1))
                : new TokenDisplayViewModelCollection();
        }

        public TokenDisplayViewModelCollection Copy()
        {
            return new TokenDisplayViewModelCollection(this);
        }

        public bool Contains(TokenId tokenId)
        {
            return Items.Any(i => i.Token.TokenId.IdEquals(tokenId));
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

        public void Replace(TokenId tokenId, Token replacementToken)
        {
            var existing = Items.FirstOrDefault(i => i.Token.TokenId.IdEquals(tokenId));
            if (existing != null)
            {
                var index = IndexOf(existing);
                Items.Insert(IndexOf(existing), new TokenDisplayViewModel(replacementToken));
                Items.Remove(existing);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CombinedNotes = new NoteViewModelCollection();
            foreach (var token in Items)
            {
                foreach (var note in token.TokenNotes)
                {
                    if (!CombinedNotes.Contains(note))
                    {
                        CombinedNotes.Add(note);
                    }
                }
                foreach (var note in token.TranslationNotes)
                {
                    if (!CombinedNotes.Contains(note))
                    {
                        CombinedNotes.Add(note);
                    }
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedNotes)));


            CombinedSurfaceText = string.Join(", ", Items.Distinct().Select(t => t.SurfaceText));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CombinedSurfaceText)));

            RecalculateEntityIds();
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(NoteIds)));
        }

        private void RecalculateEntityIds()
        {
            var previousEntityIds = EntityIds.Select(t => t.Id);

            EntityIds = new EntityIdCollection(Items.Where(t => t.IsTokenSelected).Select(t => t.Token.TokenId).Distinct());
            EntityIds.AddRange(Items
                .Where(t => t.IsTranslationSelected && t.Translation?.TranslationId != null)
                .Select(t => t.Translation!.TranslationId!)
                .Distinct());

            if (!previousEntityIds.All(o => EntityIds.Any(w => w.Id == o)))
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(EntityIds)));
            }
        }

        private IEnumerable<TokenDisplayViewModel> SelectedTokens => Items.Where(i => i.IsTokenSelected);
        private IEnumerable<TokenDisplayViewModel> SelectedTranslations => Items.Where(i => i.IsTranslationSelected);
        
        private int SelectedTokenVersesCount => SelectedTokens.Select(t => t.VerseDisplay).Distinct(ReferenceEqualityComparer.Instance).Count();
        private int SelectedTokenCompositeTokenCount => SelectedTokens.Select(t => t.CompositeToken).Distinct(ReferenceEqualityComparer.Instance).Count();

        public IEnumerable<TokenDisplayViewModel> MatchingTokens(IEnumerable<IId> entityIds)
        {
            return Items.Where(t => 
                entityIds.Contains(t.Token.TokenId, new IIdEqualityComparer()) || 
                entityIds.Contains(t.Translation?.TranslationId, new IIdEqualityComparer()));
        }

        public IEnumerable<TokenDisplayViewModel> MatchingTokens(IEnumerable<TokenDisplayViewModel> tokens)
        {
            return Items.Where(t => Items.Contains(t));
        }

        private IEnumerable<TokenDisplayViewModel> MatchingTokens(Func<TokenDisplayViewModel, bool> conditional)
        {
            return Items.Where(conditional);
        }

        private IEnumerable<TokenDisplayViewModel> NonMatchingTokens(IEnumerable<IId> entityIds)
        {
            return Items.Where(t => 
                ! entityIds.Contains(t.Token.TokenId, new IIdEqualityComparer()) &&
                ! entityIds.Contains(t.Translation?.TranslationId, new IIdEqualityComparer()));
        }

        public void MatchingTokenAction(IEnumerable<IId> entityIds, Action<TokenDisplayViewModel> action)
        {
            foreach (var token in MatchingTokens(entityIds))
            {
                action(token);
            }
        }

        public void MatchingTokenAction(IEnumerable<TokenDisplayViewModel> tokens, Action<TokenDisplayViewModel> action)
        {
            foreach (var token in MatchingTokens(tokens))
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

        public void SelectAllTokens()
        {
            foreach (var token in Items)
            {
                token.IsTokenSelected = true;
            }
        }

        public void DeselectAllTokens()
        {
            foreach (var token in Items)
            {
                token.IsTokenSelected = false;
            }
        }

        public void SelectTokens(IEnumerable<TokenDisplayViewModel> tokens)
        {
            MatchingTokenAction(tokens, t => t.IsTokenSelected = true);
        }        
        
        public void DeselectTokens(IEnumerable<TokenDisplayViewModel> tokens)
        {
            MatchingTokenAction(tokens, t => t.IsTokenSelected = false);
        }

        public int SelectedAssignedTranslationCount => SelectedTranslations
            .GroupBy(t => t.CompositeToken?.TokenId?.Id?? t.Token.TokenId.Id)
            .Count(g => g.Any(e => e.Translation?.TranslationId!=null));
        
        public int SelectedUnassignedTranslationCount => SelectedTranslations
            .GroupBy(t => t.CompositeToken?.TokenId?.Id?? t.Token.TokenId.Id)
            .Count(g => g.All(e => e.Translation?.TranslationId==null));


        public bool CanJoinTokens => SelectedTokens.Count() > 1 
                                     && SelectedTokens.All(t => t.IsSource) 
                                     && SelectedTokens.Count(t => t.IsCompositeTokenMember) <= 1
                                     && SelectedTokenVersesCount == 1 
                                     && !SelectedTranslations.Any();

        // TODO808: Completed
        // 2. in a parallel view, allow the creating of a Composite(parallel) on top of a Composite(null) (but not other Composite(parallel))
        //
        // Currently the JoinTokens and JoinTokensLanguagePair menu items use the same visibility logic.  TokenDisplayViewModelCollection should get a 
        // new CanJoinTokensLanguagePair property that is mostly similar to CanJoinTokens, but also incorporates the possibility of containing a Composite(null).
        // I'd recommend implementing this with a new TokenDisplayViewModel.IsParallelCompositeTokenMember property, which would require the addition of a
        // CompositeToken.IsParallel property or similar.  Then the above logic would be the same, just replacing:
        //
        //      && SelectedTokens.Count(t => t.IsCompositeTokenMember) <= 1
        //
        // with:
        //
        //      && SelectedTokens.Count(t => t.IsParallelCompositeTokenMember) <= 1

        public bool CanJoinTokensLanguagePair => SelectedTokens.Count() > 1
                                                && SelectedTokens.All(t => t.IsSource)
                                                && SelectedTokens.Count(t => t.IsParallelCompositeTokenMember) <= 1
                                                && SelectedTokenVersesCount == 1
                                                && !SelectedTranslations.Any();

        public bool CanUnjoinToken => SelectedTokens.All(t => t.IsCompositeTokenMember) && SelectedTokenCompositeTokenCount == 1;

        public bool CanCreateAlignment => SourceTokenCount == 1 && TargetTokenCount == 1;
        public bool CanDeleteAlignment => SelectedTokens.Any(t => t.IsHighlighted);

        public int SelectedTokenCount => SelectedTokens.Select(t => t.AlignmentToken).Distinct().Count();
        public int SourceTokenCount => SelectedTokens.Where(t => t.IsSource).Select(t => t.AlignmentToken).Distinct().Count();
        public int TargetTokenCount => SelectedTokens.Where(t => t.IsTarget).Select(t => t.AlignmentToken).Distinct().Count();

        public bool CanTranslateToken => SelectedTranslations.Count() == 1
                                         || (SelectedTranslations.Where(t=>t.CompositeToken!=null)
                                             .Select(t=>t.CompositeToken!.TokenId.Id).Distinct().Count() == 1 
                                             && SelectedTranslations.All(t=> t.CompositeToken != null));

      
    }
}
