using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
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

        public bool IsSource { get; set; }

        /// <summary>
        /// Padding to be rendered before the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingBefore { get; set; } = string.Empty;

        /// <summary>
        /// Padding to be rendered after the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingAfter { get; set; } = string.Empty;

        /// <summary>
        /// The <see cref="Translation"/> associated with the token.
        /// </summary>
        public Translation? Translation { get; set; }

        /// <summary>
        /// The surface text of the token to be displayed.  
        /// </summary>
        public string SurfaceText => Token.SurfaceText;

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

        /// <summary>
        /// A list of <see cref="NoteViewModel"/>s for the token.
        /// </summary>
        public NoteViewModelCollection Notes { get; set; } = new();

        public NoteIdCollection NoteIds { get; set; } = new();

        private bool _isSelected;
        /// <summary>
        /// Gets or sets whether this token is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        private bool _isNoteHovered;
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
            //Notes.Add(note);
            NoteIds.AddDistinct(note.NoteId);
            //NotifyOfPropertyChange(nameof(Notes));
            NotifyOfPropertyChange(nameof(HasNote));
        }

        public void NoteDeleted(NoteViewModel note)
        {
            //Notes.Remove(note);
            NoteIds.RemoveIfExists(note.NoteId);
            //NotifyOfPropertyChange(nameof(Notes));
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
