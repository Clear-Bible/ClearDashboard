using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    /// <summary>
    /// FIXME:  perhaps AlignmentOriginationFilterMode used in the non-denormalized alignment
    /// portion of this query should come from a dashboard setting/configuration
    /// </summary>
    /// <param name="TranslationSetId"></param>
    /// <param name="TokenIds"></param>
    /// <param name="AlignmentOriginationFilterMode"></param>
    public record GetTranslationsByTranslationSetIdAndTokenIdsQuery(
        TranslationSetId TranslationSetId, 
        IEnumerable<TokenId> TokenIds, 
        AlignmentOriginationFilterMode AlignmentOriginationFilterMode = AlignmentOriginationFilterMode.All) : 
        ProjectRequestQuery<IEnumerable<Alignment.Translation.Translation>>;
}
