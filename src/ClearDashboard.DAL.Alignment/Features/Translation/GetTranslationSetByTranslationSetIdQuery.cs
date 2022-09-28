using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetTranslationSetByTranslationSetIdQuery(TranslationSetId TranslationSetId) : 
        ProjectRequestQuery<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId)>;
}
