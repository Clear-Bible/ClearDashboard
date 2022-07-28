using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    /// <summary>
    /// Creates a new Corpus, associated CorpusVersion,  a new associated TokenizedCorpus, and all the tokens within the corpus. 
    /// </summary>
    /// <param name="TextCorpus"></param>
    /// <param name="CorpusId"></param>
    /// <param name="TokenizationFunction"></param>
    public record CreateTokenizedCorpusFromTextCorpusCommand(
        ITextCorpus TextCorpus, 
        CorpusId CorpusId,
        string TokenizationFunction) : ProjectRequestCommand<TokenizedTextCorpus>;
}
