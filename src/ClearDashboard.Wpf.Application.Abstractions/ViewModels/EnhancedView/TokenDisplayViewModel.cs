﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using Token = ClearBible.Engine.Corpora.Token;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;

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

        public bool IsCorpusView => VerseDisplay is CorpusDisplayViewModel;

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

        public TokenizedTextCorpus? Corpus => IsSource ? VerseDisplay.SourceCorpus : VerseDisplay.TargetCorpus;

        /// <summary>
        /// Placeholder for when we will need to support RTL for the selected application language.
        /// </summary>
        public FlowDirection TooltipFlowDirection => FlowDirection.LeftToRight;

        public string AlignmentTooltip
        {
            get
            {

                var localization = IoC.Get<ILocalizationService>();

                if (IsInvalidAlignment)
                {
                    return localization["BulkAlignmentReview_Invalid"];
                }

                if (IsValidAlignment)
                {
                    return localization["BulkAlignmentReview_Valid"];
                }

                if (IsNeedReviewAlignment)
                {
                    return localization["BulkAlignmentReview_NeedsReview"];
                }

                return localization["BulkAlignmentReview_Machine"]; ;
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
                                                                                         && a.OriginatedFrom == "Assigned" );

        public bool IsValidAlignment => VerseDisplay is AlignmentDisplayViewModel
                                         && VerseDisplay.AlignmentManager is { Alignments: { } }
                                         && VerseDisplay.AlignmentManager.Alignments.Any(a =>
                                             (a.AlignedTokenPair.SourceToken.TokenId.Id == AlignmentToken.TokenId.Id || a.AlignedTokenPair.TargetToken.TokenId.Id == AlignmentToken.TokenId.Id)
                                             && a.OriginatedFrom == "Assigned" && a.Verification == AlignmentVerificationStatus.Verified);

        public bool IsInvalidAlignment => VerseDisplay is AlignmentDisplayViewModel
                                        && VerseDisplay.AlignmentManager is { Alignments: { } }
                                        && VerseDisplay.AlignmentManager.Alignments.Any(a =>
                                            (a.AlignedTokenPair.SourceToken.TokenId.Id == AlignmentToken.TokenId.Id || a.AlignedTokenPair.TargetToken.TokenId.Id == AlignmentToken.TokenId.Id)
                                            && a.OriginatedFrom == "Assigned" && a.Verification == AlignmentVerificationStatus.Invalid);

        public bool IsNeedReviewAlignment => VerseDisplay is AlignmentDisplayViewModel
                                          && VerseDisplay.AlignmentManager is { Alignments: { } }
                                          && VerseDisplay.AlignmentManager.Alignments.Any(a =>
                                              (a.AlignedTokenPair.SourceToken.TokenId.Id == AlignmentToken.TokenId.Id || a.AlignedTokenPair.TargetToken.TokenId.Id == AlignmentToken.TokenId.Id)
                                              && a.OriginatedFrom == "Assigned" && a.Verification == AlignmentVerificationStatus.Unverified);

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
        /// Gets whether this is token is part of a 'parallel' composite token as determined by the HasTag property of the token.
        /// </summary>
        public bool IsParallelCompositeTokenMember => IsCompositeTokenMember &&
                                                      CompositeToken!.HasMetadatum(MetadatumKeys.IsParallelCompositeToken)
                                                      && CompositeToken!.GetMetadatum<bool>(MetadatumKeys.IsParallelCompositeToken);

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


        private BindableCollection<ExternalNote> _externalNotes = new();
        public BindableCollection<ExternalNote> ExternalNotes
        {
            get => _externalNotes;
            set
            {
                if (Set(ref _externalNotes, value))
                {
                    NotifyOfPropertyChange(nameof(ExternalNotes));
                    NotifyOfPropertyChange(nameof(HasExternalNotes));
                }
            }
        }
        public void NotifyExternalNotesItemsChanged()
        {
            NotifyOfPropertyChange(nameof(ExternalNotes));
            NotifyOfPropertyChange(nameof(HasExternalNotes));
        }

        /// <summary>
        /// The surface text of the token to be displayed.  
        /// </summary>
        public string SurfaceText => Token.SurfaceText;

        /// <summary>
        /// The training text of the token to be displayed.  
        /// </summary>
        public string TrainingText => Token.TrainingText;

        /// <summary>
        /// The surface text of the token to be displayed for translations.
        /// </summary>
        public string TranslationSurfaceText => IsCompositeTokenMember ? string.Join(" ", CompositeToken!.Tokens.Select(t => t.SurfaceText))
                                                                       : Token.SurfaceText;

        /// <summary>
        /// The training text of the token to be displayed for translations.
        /// </summary>
        public string TranslationTrainingText => IsCompositeTokenMember ? string.Join(" ", CompositeToken!.Tokens.Select(t => t.TrainingText))
                                                                       : Token.TrainingText;

        /// <summary>
        /// The surface and training text of the token to be displayed for translations.
        /// </summary>
        public string TranslationSurfaceAndTrainingText => $"{TranslationSurfaceText} ({TranslationTrainingText})";

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
        public NoteViewModelCollection TokenNotes { get; set; } = new();

        public NoteIdCollection TokenNoteIds { get; set; } = new();

        private bool _isTokenSelected;
        /// <summary>
        /// Gets or sets whether this token is selected.
        /// </summary>
        public bool IsTokenSelected
        {
            get => _isTokenSelected;
            set => Set(ref _isTokenSelected, value);
        }

        /// <summary>
        /// A list of <see cref="NoteViewModel"/>s for the token.
        /// </summary>
        public NoteViewModelCollection TranslationNotes { get; set; } = new();

        public NoteIdCollection TranslationNoteIds { get; set; } = new();

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

        private bool _isTranslationNoteHovered;
        public bool IsTranslationNoteHovered
        {
            get => _isTranslationNoteHovered;
            set => Set(ref _isTranslationNoteHovered, value);
        }

        public SolidColorBrush NoteIndicatorBrush
        {
            get => _noteIndicatorBrush;
            set => Set(ref _noteIndicatorBrush, value);
        }

        public bool TokenHasNote => TokenNoteIds.Any();
        public bool TranslationHasNote => TranslationNoteIds.Any();

        public void TokenNoteAdded(NoteViewModel note)
        {
            TokenNoteIds.AddDistinct(note.NoteId!);
            NotifyOfPropertyChange(nameof(TokenHasNote));
        }

        public void TokenNoteDeleted(NoteViewModel note)
        {
            TokenNoteIds.RemoveIfExists(note.NoteId!);
            NotifyOfPropertyChange(nameof(TokenHasNote));
        }
        public bool HasExtendedProperties => !string.IsNullOrEmpty(ExtendedProperties);
        public bool HasExternalNotes => ExternalNotes.Count() > 0 && AbstractionsSettingsHelper.GetShowExternalNotes();


        private bool _isFirstExternalNoteToken = false;
        public bool IsFirstExternalNoteToken
        {
            get => _isFirstExternalNoteToken && AbstractionsSettingsHelper.GetShowExternalNotes();
            set 
            { 
                _isFirstExternalNoteToken = value;
                NotifyOfPropertyChange(nameof(IsFirstExternalNoteToken));
            }
        }

        private bool _isFirstJotsNoteToken = false;
        public bool IsFirstJotsNoteToken
        {
            get => _isFirstJotsNoteToken;
            set
            {
                _isFirstJotsNoteToken = value;
                NotifyOfPropertyChange(nameof(IsFirstJotsNoteToken));
                NotifyOfPropertyChange(nameof(TokenHasNote));
            }
        }

        private bool _isFirstJotsNoteTranslation = false;
        public bool IsFirstJotsNoteTranslation
        {
            get => _isFirstJotsNoteTranslation;
            set
            {
                _isFirstJotsNoteTranslation = value;
                NotifyOfPropertyChange(nameof(IsFirstJotsNoteTranslation));
                NotifyOfPropertyChange(nameof(TranslationHasNote));
            }
        }

        private bool _multipleExternalNotes = false;
        private SolidColorBrush _noteIndicatorBrush;

        public bool MultipleExternalNotes
        {
            get => _multipleExternalNotes && AbstractionsSettingsHelper.GetShowExternalNotes();
            set 
            { 
                _multipleExternalNotes = value;
                NotifyOfPropertyChange(nameof(MultipleExternalNotes));
            }
        }

        public void OnToolTipOpening(ToolTipEventArgs e)
        {
            if (!IsHighlighted && string.IsNullOrWhiteSpace(ExtendedProperties))
            {
                e.Handled = true;
            }
        }


        public void TranslationNoteAdded(NoteViewModel note)
        {
            TranslationNoteIds.AddDistinct(note.NoteId!);
            NotifyOfPropertyChange(nameof(TranslationHasNote));
        }

        public void TranslationNoteDeleted(NoteViewModel note)
        {
            TranslationNoteIds.RemoveIfExists(note.NoteId!);
            NotifyOfPropertyChange(nameof(TranslationHasNote));
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
            NoteIndicatorBrush = Brushes.LightGray;
        }
    }
}
