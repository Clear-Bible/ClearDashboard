using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Engine.Corpora;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record CreateParallelTokenizedCorpusCommand(
        ParallelCorpusVersionId ParallelCorpusVersionId,
        TokenizedCorpusId SourceTokenizedCorpusId,
        TokenizedCorpusId TargetTokenizedCorpusId) 
        : IRequest<RequestResult<ParallelTokenizedCorpusId>>;
}
