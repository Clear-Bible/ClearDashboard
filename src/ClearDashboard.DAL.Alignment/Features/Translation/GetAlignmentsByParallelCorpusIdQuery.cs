using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentsByParallelCorpusIdQuery(ParallelCorpusId ParallelCorpusId) : IRequest<RequestResult<IEnumerable<(Token, Token, double)>?>>;
}
