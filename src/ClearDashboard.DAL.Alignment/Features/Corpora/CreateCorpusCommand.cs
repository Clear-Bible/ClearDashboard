using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
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
    public record CreateCorpusCommand(
        bool IsRtl,
        string FontFamily,
        string Name, 
        string Language, 
        string CorpusType,
        string ParatextId) : ProjectRequestCommand<CorpusId>;
}
