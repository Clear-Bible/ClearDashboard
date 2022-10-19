using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Translation;

public class Translation
{
    public TranslationId? TranslationId { get; private set; }
    public Token SourceToken { get; private set; }
    public string TargetTranslationText { get; set; }
    /// <summary>
    /// Possible values are:  "FromOther", "Assigned", "FromTranslationModel", "FromAlignmentModel"
    /// </summary>
    public string OriginatedFrom { get; set; }

    /// <summary>
    /// 
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