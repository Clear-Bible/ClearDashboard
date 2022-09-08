using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation;

public record Translation(Token SourceToken, string TargetTranslationText, string TranslationOriginatedFrom);