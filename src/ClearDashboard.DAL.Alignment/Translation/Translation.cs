using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation;

/// <summary>
/// 
/// </summary>
/// <param name="SourceToken"></param>
/// <param name="TargetTranslationText"></param>
/// <param name="TranslationOriginatedFrom">Valid values are:  "FromOther", "Assigned" only</param>
public record Translation(Token SourceToken, string TargetTranslationText, string TranslationOriginatedFrom);