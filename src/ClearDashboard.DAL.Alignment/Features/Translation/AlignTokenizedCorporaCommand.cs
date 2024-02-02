using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Machine.Corpora;
using static ClearBible.Engine.Persistence.FileGetBookIds;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record AlignTokenizedCorporaCommand(
        // Oneof:
        ITextCorpus? SourceTokenizedTextCorpus,
        LanguageCodeEnum? SourceManuscriptCorpusType,
        // Oneof:
        ITextCorpus? TargetTokenizedTextCorpus,
		LanguageCodeEnum? TargetManuscriptCorpusType,
		bool IsTrainedSymmetrizedModel,
		SmtModelType SmtModelType,
	    IEnumerable<VerseMapping>? VerseMappings = null) : IRequest<RequestResult<(
            IEnumerable<AlignedTokenPairs> AlignTokenPairs, 
            Dictionary<string, Dictionary<string, double>> TranslationModel
        )>>;
}
