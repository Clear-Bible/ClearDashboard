using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelTokenizedCorpusCommand(string ProjectName,
        ParallelCorpusVersionId ParallelCorpusVersionId,
        TokenizedCorpusId SourceTokenizedCorpusId,
        TokenizedCorpusId TargetTokenizedCorpusId) 
        : ProjectRequestCommand<ParallelTokenizedCorpusId>(ProjectName);
}
