using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    /// <summary>
    /// A class containing the needed information to render a <see cref="Token"/> in the UI.
    /// </summary>
    public class TokenDisplay
    {
        /// <summary>
        /// The token itself.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Padding to be rendered before the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingBefore { get; set; }

        /// <summary>
        /// Padding to be rendered after the token, as determined by a <see cref="EngineStringDetokenizer"/>.
        /// </summary>
        public string PaddingAfter { get; set; }

        /// <summary>
        /// The <see cref="Translation"/> associated with the token.
        /// </summary>
        public Translation? Translation { get; set; }

        /// <summary>
        /// The surface text of the token to be displayed.
        /// </summary>
        public string SurfaceText => Token.SurfaceText;

        /// <summary>
        /// The target translation text of the token.
        /// </summary>
        public string TargetTranslationText => Translation?.TargetTranslationText;

        /// <summary>
        /// The <see cref="TranslationState"/> of the translation.
        /// </summary>
        public string TranslationState => Translation?.TranslationState;

        public string? Note { get; set; }
    }
}
