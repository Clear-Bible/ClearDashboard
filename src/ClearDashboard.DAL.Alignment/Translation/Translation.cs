using System.Reflection.Metadata.Ecma335;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Translation;

public class Translation
{
    public class OriginatedFromValues
    {
        public const string Assigned = "Assigned";
        public const string FromAlignmentModel = "FromAlignmentModel";
        public const string FromTranslationModel = "FromTranslationModel";
        public const string FromOther = "FromOther";
        public const string None = "None";
    }

    private const string DefaultTranslationText = "___";

    public TranslationId? TranslationId { get; private set; }
    public Token SourceToken { get; private set; }
    public string TargetTranslationText { get; set; }
    
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
    public Translation(Token sourceToken, string targetTranslationText, string originatedFrom)
    {
        SourceToken = sourceToken;
        TargetTranslationText = targetTranslationText;
        OriginatedFrom = originatedFrom;
    }

    internal Translation(TranslationId translationId, Token sourceToken, string targetTranslationText, string translationOriginatedFrom)
    {
        TranslationId = translationId;
        SourceToken = sourceToken;
        TargetTranslationText = targetTranslationText;
        OriginatedFrom = translationOriginatedFrom;
    }
}