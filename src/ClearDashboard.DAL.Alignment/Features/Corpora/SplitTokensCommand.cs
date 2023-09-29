using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record SplitTokensCommand(
        TokenizedTextCorpusId TokenizedTextCorpusId,
        IEnumerable<TokenId> TokenIdsWithSameSurfaceText,
        int SurfaceTextIndex,
        int SurfaceTextLength,
        string TrainingText1,
        string TrainingText2,
        string? TrainingText3,
        bool CreateParallelComposite = true,
        SplitTokenPropagationScope PropagateTo = SplitTokenPropagationScope.None) : 
        ProjectRequestCommand<
            (IDictionary<TokenId, IEnumerable<CompositeToken>> SplitCompositeTokensByIncomingTokenId, 
             IDictionary<TokenId, IEnumerable<Token>> SplitChildTokensByIncomingTokenId)>;

    public enum SplitTokenPropagationScope
    {
        None,
        BookChapterVerse,
        BookChapter,
        Book,
        All
    }
}
