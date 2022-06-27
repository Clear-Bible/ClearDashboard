using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;

namespace ClearBible.Alignment.DataServices.Features.Translation
{
    public record GetAlignmentsByParallelCorpusIdQuery(ParallelCorpusId ParallelCorpusId) : IRequest<RequestResult<IEnumerable<(Token, Token, double)>?>>;
}
