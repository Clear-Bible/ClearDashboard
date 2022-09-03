using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetTranslationsByTranslationSetIdAndTokenIdRangeQuery(TranslationSetId TranslationSetId, TokenId FirstTokenId, TokenId LastTokenId) : 
        ProjectRequestQuery<IEnumerable<Alignment.Translation.Translation>>;
}
