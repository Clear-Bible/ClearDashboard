using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using SIL.Machine.Corpora;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    /// <summary>
    /// Creates a new Corpus, associated CorpusVersion,  a new associated TokenizedCorpus, and all the tokens within the corpus. 
    /// </summary>
    /// <param name="TextCorpus"></param>
    /// <param name="IsRtl"></param>
    /// <param name="Name"></param>
    /// <param name="Language"></param>
    /// <param name="CorpusType"></param>
    public record CreateTokenizedCorpusFromTextCorpusCommand(
        ITextCorpus TextCorpus, 
        bool IsRtl, 
        string Name, 
        string Language, 
        string CorpusType,
        string TokenizationQueryString) : IRequest<RequestResult<TokenizedTextCorpus>>;
}
