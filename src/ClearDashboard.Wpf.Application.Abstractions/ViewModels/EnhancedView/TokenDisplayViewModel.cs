using System;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A class containing the needed information to render a <see cref="Token"/> in the UI.
    /// </summary>
    public class TokenDisplayViewModel : PropertyChangedBase
    {
        /// <summary>
        /// The token itself.
        /// </summary>
        public Token Token { get; }

        public Token TokenForTranslation => IsCompositeTokenMember ? CompositeToken! : Token;

        public Token AlignmentToken => IsCompositeTokenMember ? CompositeToken! : Token;

        /// <summary>
        /// The <see cref="VerseDisplayViewModel"/> that this token is part of.
        /// </summary>
        public VerseDisplayViewModel VerseDisplay { get; set; }

        public bool IsSourceRtl => VerseDisplay.IsSourceRtl;

        public bool IsTargetRtl => VerseDisplay.IsTargetRtl;

        /// <summary>
        /// Gets or sets whether this is a source token.
        /// </summary>
        /// <remarks>
        /// If true, this is a source token; if false, this is a target token (alignment).
        /// </remarks>
        public bool IsSource
        {
            get => _isSource;
            set
            {
                Set(ref _isSource, value);
                NotifyOfPropertyChange(nameof(IsTarget));
            }
        }

        /// <summary>
        /// Gets or sets whether this is a source token.
        /// </summary>
        public bool IsTarget => !IsSource;

        public bool IsAligned => VerseDisplay.AlignmentManager is { Alignments: { } } &&
                                 VerseDisplay.AlignmentManager.Alignments.Any(a => a.AlignedTokenPair.SourceToken.TokenId.Id == AlignmentToken.TokenId.Id 
                                     || a.AlignedTokenPair.TargetToken.TokenId.Id == AlignmentToken.TokenId.Id);
        public bool IsManualAlignment => VerseDisplay is AlignmentDisplayViewModel 
                                         && VerseDisplay.AlignmentManager is { Alignments: { } } 
                                         && VerseDisplay.AlignmentManager.Alignments.Any(a => 
                                                                                         (a.AlignedTokenPair.SourceToken.TokenId.Id == AlignmentToken.TokenId.Id || a.AlignedTokenPair.TargetToken.TokenId.Id == AlignmentToken.TokenId.Id)
                                                                                         && a.OriginatedFrom == "Assigned");

        private CompositeToken? _compositeToken;
        /// <summary>
        /// Gets or sets the parent <see cref="CompositeToken"/> of this token, if any.
        /// </summary>
        public CompositeToken? CompositeToken
        {
            get => _compositeToken;
            set
            {
                if (Set(ref _compositeToken, value))
                {
                    CompositeTokenMembers = _compositeToken != null
                        ? new TokenCollection(_compositeToken.Tokens)
                        : new TokenCollection();
                    NotifyOfPropertyChange(nameof(CompositeTokenMembers));
                    NotifyOfPropertyChange(nameof(IsCompositeTokenMember));
                    NotifyOfPropertyChange(nameof(CompositeIndicatorColor));
                }
            }
        }

        /// <summary>
        /// Gets whether this is token is part of a composite token.
        /// </summary>
        public bool IsCompositeTokenMember => CompositeToken != null;

        /// <summary>
        /// Gets a collection of the composite token members.
        /// </summary>
        public TokenCollection CompositeTokenMembers { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to use when displaying a composite token indicator.
        /// </summary>
        public Brush CompositeIndicatorColor => CompositeTokenColors.Get(CompositeToken);

        /// <summary>
        /// Padding to be rendered before the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingBefore { get; set; } = string.Empty;

        /// <summary>
        /// Padding to be rendered after the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingAfter { get; set; } = string.Empty;

        private Translation? _translation;

        /// <summary>
        /// The <see cref="Translation"/> associated with the token.
        /// </summary>
        public Translation? Translation
        {
            get => _translation;
            set
            {
                if (Set(ref _translation, value))
                {
                    NotifyOfPropertyChange(nameof(TargetTranslationText));
                    NotifyOfPropertyChange(nameof(TranslationState));
                }
            }
        }

        /// <summary>
        /// The surface text of the token to be displayed.  
        /// </summary>
        public string SurfaceText => Token.SurfaceText;

        /// <summary>
        /// The surface text of the token to be displayed for translations.
        /// </summary>
        public string TranslationSurfaceText => IsCompositeTokenMember ? string.Join(" ", CompositeToken!.Tokens.Select(t => t.SurfaceText))
                                                                       : Token.SurfaceText;

        /// <summary>
        /// The extended properties of the token to be displayed.
        /// </summary>
        public string? ExtendedProperties => Token.ExtendedProperties;

        /// <summary>
        /// The target translation text of the token.
        /// </summary>
        public string TargetTranslationText => Translation?.TargetTranslationText ?? string.Empty;

        /// <summary>
        /// The <see cref="TranslationState"/> of the translation.
        /// </summary>
        public string TranslationState => Translation?.OriginatedFrom ?? string.Empty;

        private Token? _alignedToken;

        /// <summary>
        /// The <see cref="Token"/> that is aligned to the token.
        /// </summary>
        public Token? AlignedToken
        {
            get => _alignedToken;
            set
            {
                if (Set(ref _alignedToken, value))
                {
                    NotifyOfPropertyChange(nameof(AlignedTokenSurfaceText));
                }
            }
        }

        /// <summary>
        /// The text of the token aligned with this token.
        /// </summary>
        public string AlignedTokenSurfaceText => AlignedToken?.SurfaceText ?? string.Empty;

        /// <summary>
        /// A list of <see cref="NoteViewModel"/>s for the token.
        /// </summary>
        public NoteViewModelCollection Notes { get; set; } = new();

        public NoteIdCollection NoteIds { get; set; } = new();

        private bool _isTokenSelected;
        /// <summary>
        /// Gets or sets whether this token is selected.
        /// </summary>
        public bool IsTokenSelected
        {
            get => _isTokenSelected;
            set => Set(ref _isTokenSelected, value);
        }        
        
        private bool _isTranslationSelected;
        /// <summary>
        /// Gets or sets whether the translation for this token is selected.
        /// </summary>
        public bool IsTranslationSelected
        {
            get => _isTranslationSelected;
            set => Set(ref _isTranslationSelected, value);
        }

        private bool _isAlignmentSelected;
        /// <summary>
        /// Gets or sets whether the alignment for this token is selected.
        /// </summary>
        public bool IsAlignmentSelected
        {
            get => _isAlignmentSelected;
            set => Set(ref _isAlignmentSelected, value);
        }

        private bool _isHighlighted;
        /// <summary>
        /// Gets or sets whether this token is highlighted.
        /// </summary>
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => Set(ref _isHighlighted, value);
        }

        private bool _isNoteHovered;
        private bool _isSource = true;

        /// <summary>
        /// Gets or sets whether a note to which the token is associated is hovered by the mouse.
        /// </summary>
        public bool IsNoteHovered
        {
            get => _isNoteHovered;
            set => Set(ref _isNoteHovered, value);
        }

        public bool HasNote => NoteIds.Any();

        public void NoteAdded(NoteViewModel note)
        {
            NoteIds.AddDistinct(note.NoteId!);
            NotifyOfPropertyChange(nameof(HasNote));
        }

        public void NoteDeleted(NoteViewModel note)
        {
            NoteIds.RemoveIfExists(note.NoteId!);
            NotifyOfPropertyChange(nameof(HasNote));
        }

        public void TranslationApplied(Translation translation)
        {
            Translation = translation;
            NotifyOfPropertyChange(nameof(Translation));
            NotifyOfPropertyChange(nameof(TargetTranslationText));
            NotifyOfPropertyChange(nameof(TranslationState));
        }

        public TokenDisplayViewModel(Token token)
        {
            Token = token;
        }
    }
}
