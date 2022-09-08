using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    /// <summary>
    /// Creates a new Corpus, associated CorpusVersion,  a new associated TokenizedCorpus, and all the tokens within the corpus. 
    /// </summary>
    /// <param name="TextCorpus"></param>
    /// <param name="CorpusId"></param>
    /// <param name="DisplayName"></param>
    /// <param name="TokenizationFunction"></param>
    /// <param name="Versification"></param>
    public record CreateTokenizedCorpusFromTextCorpusCommand(
        ITextCorpus TextCorpus, 
        CorpusId CorpusId,
        string? DisplayName,
        string? TokenizationFunction,
        ScrVers Versification) : ProjectRequestCommand<TokenizedTextCorpus>;
}
