using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    /* IMPLEMENTER'S NOTES:
     * return value is ignored. Marked as object to accommodate compilation needs of RequestResult only.
     * 
     */
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceTokenToTargetTokenAlignments"></param>
    /// <param name="parallelCorpusId"></param>
    public record PutAlignmentsCommand(IEnumerable<(Token, Token, double)> sourceTokenToTargetTokenAlignments, ParallelCorpusId parallelCorpusId) : IRequest<RequestResult<object>>;
}
