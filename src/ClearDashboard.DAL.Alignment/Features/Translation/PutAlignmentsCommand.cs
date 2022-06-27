using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;

namespace ClearBible.Alignment.DataServices.Features.Corpora
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
