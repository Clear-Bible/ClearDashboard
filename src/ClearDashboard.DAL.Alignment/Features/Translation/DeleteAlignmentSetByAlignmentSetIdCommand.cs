using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    /// <summary>
    /// Deletes an AlignmentSet, as well as any/all downstream dependent 
    /// instances (e.g. TranslationSets, Translations etc)
    /// </summary>
    /// <param name="AlignmentSetId"></param>
    public record DeleteAlignmentSetByAlignmentSetIdCommand(AlignmentSetId AlignmentSetId) : ProjectRequestCommand<Unit>;
}
