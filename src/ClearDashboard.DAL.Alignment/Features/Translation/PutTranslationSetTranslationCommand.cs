using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record PutTranslationSetTranslationCommand(
        TranslationSetId TranslationSetId,
        Alignment.Translation.Translation Translation,
        string TranslationActionType) : ProjectRequestCommand<object>;
}