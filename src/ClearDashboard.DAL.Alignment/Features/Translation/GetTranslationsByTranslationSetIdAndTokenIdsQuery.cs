using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    /// <summary>
    /// </summary>
    /// <param name="TranslationSetId"></param>
    /// <param name="TokenIds"></param>
    /// <param name="AlignmentOriginationFilterMode"></param>
    public record GetTranslationsByTranslationSetIdAndTokenIdsQuery(
        TranslationSetId TranslationSetId, 
        IEnumerable<TokenId> TokenIds, 
        AlignmentTypes AlignmentTypesToInclude) : 
        ProjectRequestQuery<IEnumerable<Alignment.Translation.Translation>>;
}
