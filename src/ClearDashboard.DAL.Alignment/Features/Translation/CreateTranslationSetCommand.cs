using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record CreateTranslationSetCommand(
        Dictionary<string, Dictionary<string, double>>? TranslationModel,
        AlignmentSetId alignmentSetId,
        string? DisplayName,
        //string SmtModel,
        Dictionary<string, object> Metadata,
        ParallelCorpusId ParallelCorpusId) : ProjectRequestCommand<TranslationSet>;
}
