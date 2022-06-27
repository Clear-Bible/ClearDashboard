using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelTokenizedCorpusCommand(
        ParallelCorpusVersionId ParallelCorpusVersionId,
        TokenizedCorpusId SourceTokenizedCorpusId,
        TokenizedCorpusId TargetTokenizedCorpusId) 
        : IRequest<RequestResult<ParallelTokenizedCorpusId>>;
}
