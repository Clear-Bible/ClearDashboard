using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record PutTranslationSetModelEntryCommand(
        TranslationSetId TranslationSetId,
        string sourceText, 
        Dictionary<string, double> targetTranslationTextScores) : ProjectRequestCommand<object>;
}