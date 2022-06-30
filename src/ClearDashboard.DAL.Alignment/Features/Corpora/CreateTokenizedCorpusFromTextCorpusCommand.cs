using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
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
        string ProjectName,
        ITextCorpus TextCorpus, 
        bool IsRtl, 
        string Name, 
        string Language, 
        string CorpusType,
        string TokenizationQueryString) : ProjectRequestCommand<TokenizedTextCorpus>(ProjectName);
}
