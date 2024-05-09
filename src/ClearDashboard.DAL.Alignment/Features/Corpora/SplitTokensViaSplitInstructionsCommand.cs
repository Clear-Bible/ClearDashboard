using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public record SplitTokensViaSplitInstructionsCommand(
    TokenizedTextCorpusId TokenizedTextCorpusId,
    IEnumerable<TokenId> TokenIdsWithSameSurfaceText,
    SplitInstructions SplitInstructions,
    bool CreateParallelComposite = true,
    SplitTokenPropagationScope PropagateTo = SplitTokenPropagationScope.None) :
    ProjectRequestCommand<
        (IDictionary<TokenId, IEnumerable<CompositeToken>> SplitCompositeTokensByIncomingTokenId, IDictionary<TokenId, IEnumerable<Token>> SplitChildTokensByIncomingTokenId)>;