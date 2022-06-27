using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Engine.Corpora;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record CreateParallelCorpusCommand() 
        : IRequest<RequestResult<ParallelCorpusId>>;
}
