using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Annotations;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    /// <summary>
    /// A class containing the needed information to render a <see cref="Token"/> in the UI.
    /// </summary>
    public class TokenDisplayViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = false;

        /// <summary>
        /// The token itself.
        /// </summary>
        public Token Token { get; set; }

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
        /// The extended propertiesof the token to be displayed.
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

        /// <summary>
        /// Gets or sets whether this token is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool HasNote => Notes.Any();

        public void NoteAdded(NoteViewModel note)
        {
            Notes.Add(note);
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(HasNote));
        }

        public void NoteDeleted(NoteViewModel note)
        {
            Notes.Remove(note);
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(HasNote));
        }

        public void TranslationApplied(Translation translation)
        {
            Translation = translation;
            OnPropertyChanged(nameof(Translation));
            OnPropertyChanged(nameof(TargetTranslationText));
            OnPropertyChanged(nameof(TranslationState));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
