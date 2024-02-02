using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    /// <summary>
    /// Creates a new Corpus. 
    /// </summary>
    /// <param name="IsRtl"></param>
    /// <param name="Name"></param>
    /// <param name="Language"></param>
    /// <param name="CorpusType"></param>
    public record TokenizeTextCorpusCommand(
        ITextCorpus TextCorpus,
        IEnumerable<string> TokenizeTransformChain) : IRequest<RequestResult<ITextCorpus>>;
}
