using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetTranslationSetModelEntryQuery(TranslationSetId TranslationSetId, string SourceText) : 
        ProjectRequestQuery<Dictionary<string, double>?>;
}
 