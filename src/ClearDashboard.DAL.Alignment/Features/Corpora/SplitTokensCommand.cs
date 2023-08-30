using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record SplitTokensCommand(
        IEnumerable<TokenId> TokenIdsWithSameSurfaceText,
        int SurfaceTextIndex,
        int SurfaceTextLength,
        string TrainingText1,
        string TrainingText2,
        string? TrainingText3,
        bool CreateParallelComposite = true) : ProjectRequestCommand<(IEnumerable<CompositeToken>, IEnumerable<Token>)>;
}
