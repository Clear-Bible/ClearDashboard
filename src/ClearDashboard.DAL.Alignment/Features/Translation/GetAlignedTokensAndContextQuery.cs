using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignedTokensAndContextQuery(string sourceTrainingText) : IRequest<RequestResult<IEnumerable<(string trainingTargetText, string surfaceTargetText, IEnumerable<(IEnumerable<Token>, IEnumerable<int>)>)>>>;
}
