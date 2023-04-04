using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    /// <summary>
    /// Deletes an TranslationSet, as well as any/all downstream dependent 
    /// instances (e.g. Translations)
    /// </summary>
    /// <param name="TranslationSetId"></param>
    public record DeleteTranslationSetByTranslationSetIdCommand(TranslationSetId TranslationSetId) : ProjectRequestCommand<Unit>;
}
