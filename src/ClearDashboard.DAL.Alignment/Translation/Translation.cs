using System.Reflection.Metadata.Ecma335;
using System.Text;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Translation;

public class Translation
{
    public class OriginatedFromValues
    {
        public const string Assigned = "Assigned";
        public const string FromLexicon = "FromLexicon";
        public const string FromAlignmentModel = "FromAlignmentModel";
        public const string FromTranslationModel = "FromTranslationModel";
        public const string FromOther = "FromOther";
        public const string None = "None";
    }

    public const string DefaultTranslationText = "___";

    public TranslationId? TranslationId { get; private set; }
    public Token SourceToken { get; private set; }
    public string TargetTranslationText { get; set; }
    public Lexicon.TranslationId? LexiconTranslationId { get; set; }
    
    public string SourceTokenSurfaceText => SourceToken is CompositeToken token ? string.Join(" ", token.Tokens.Select(t => t.SurfaceText)) : SourceToken.SurfaceText;

    public byte[] SourceTokenSurfaceTextBytes => Encoding.Unicode.GetBytes(SourceTokenSurfaceText);
    public string SourceTokenSurfaceByteString => string.Join(" ", SourceTokenSurfaceTextBytes.Select(x => x.ToString("X2")));

    public byte[] TargetTranslationTextBytes => Encoding.Unicode.GetBytes(TargetTranslationText);
    public string TargetTranslationTextByteString => string.Join(" ", TargetTranslationTextBytes.Select(x => x.ToString("X2")));

    /// <summary>
    /// Possible values are:  "FromOther", "Assigned", "FromTranslationModel", "FromAlignmentModel", "None"
    /// </summary>
    public string OriginatedFrom { get; set; }

    public bool IsDefault => TargetTranslationText == DefaultTranslationText;

    /// <summary>
    /// Create a default translation for a token using a placeholder for the target translation text.
    /// </summary>
    /// <param name="sourceToken">The token for which to create the translation.</param>
    public Translation(Token sourceToken)
    {
        SourceToken = sourceToken;
        TargetTranslationText = DefaultTranslationText;
        OriginatedFrom = OriginatedFromValues.None;
    }

    /// <summary>
    /// Create a translation for a token using the specified target translation text.
    /// </summary>
    /// <param name="sourceToken"></param>
    /// <param name="targetTranslationText"></param>
    /// <param name="originatedFrom">Valid values are:  "FromOther", "Assigned" only</param>
    public Translation(Token sourceToken, string targetTranslationText, string originatedFrom, Lexicon.TranslationId? lexiconTranslationId = null)
    {
        SourceToken = sourceToken;
        TargetTranslationText = targetTranslationText;
        OriginatedFrom = originatedFrom;
        LexiconTranslationId = lexiconTranslationId;
    }

    internal Translation(TranslationId translationId, Token sourceToken, string targetTranslationText, string translationOriginatedFrom, Lexicon.TranslationId? lexiconTranslationId)
    {
        TranslationId = translationId;
        SourceToken = sourceToken;
        TargetTranslationText = targetTranslationText;
        OriginatedFrom = translationOriginatedFrom;
        LexiconTranslationId = lexiconTranslationId;
    }
}