using Autofac;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using MediatR;

namespace ClearDashboard.DAL.Alignment
{
    public class TopLevelProjectIds
    {
        // All Project TranslationSetIds
        public IEnumerable<TranslationSetId> TranslationSetIds { get; init; }

        // All Project AlignmentSetIds
        public IEnumerable<AlignmentSetId> AlignmentSetIds { get; init; }

        // ParallelCorpusIds not contained in any TranslationSetIds or AlignmentSetIds
        public IEnumerable<ParallelCorpusId> ParallelCorpusIds { get; init; }

        // TokenizedTextCorpusIds not contained in any ParallelCorpusIds
        public IEnumerable<TokenizedTextCorpusId> TokenizedTextCorpusIds { get; init; }

        // CorpusIds not contained in any TokenizedTextCorpusIds
        public IEnumerable<CorpusId> CorpusIds { get; init; }

        public TopLevelProjectIds(
            IEnumerable<TranslationSetId> translationSetIds,
            IEnumerable<AlignmentSetId> alignmentSetIds,
            IEnumerable<ParallelCorpusId> parallelCorpusIds,
            IEnumerable<TokenizedTextCorpusId> tokenizedTextCorpusIds,
            IEnumerable<CorpusId> corpusIds)
        {
            TranslationSetIds = translationSetIds;
            AlignmentSetIds = alignmentSetIds;
            ParallelCorpusIds = parallelCorpusIds;
            TokenizedTextCorpusIds = tokenizedTextCorpusIds;
            CorpusIds = corpusIds;
        }

        public static async Task<TopLevelProjectIds> GetTopLevelProjectIdsAsync(IComponentContext context, CancellationToken cancellationToken)
        {
            var mediator = context.Resolve<IMediator>();

            var translationSetIds = await TranslationSet.GetAllTranslationSetIdsAsync(context, null, null, cancellationToken);
            var alignmentSetIds = await AlignmentSet.GetAllAlignmentSetIdsAsync(context, null, null, cancellationToken);

            var allParallelCorpusIds = await ParallelCorpus.GetAllParallelCorpusIdsAsync(context, cancellationToken);
            var containedParallelCorpusIds = 
                translationSetIds
                    .Select(e => e.ParallelCorpusId!.Id)
                .Union(alignmentSetIds
                    .Select(e => e.ParallelCorpusId!.Id))
                .Distinct();

            var parallelCorpusIds = allParallelCorpusIds
                .Where(e => containedParallelCorpusIds.Contains(e.Id));

            var allTokenizedTextCorpusIds = await TokenizedTextCorpus.GetAllTokenizedCorpusIdsAsync(context, null, cancellationToken);
            var containedTokenizedTextCorpusIds =
                allParallelCorpusIds
                    .Select(e => e.SourceTokenizedCorpusId!.Id)
                .Union(allParallelCorpusIds
                    .Select(e => e.TargetTokenizedCorpusId!.Id))
                .Distinct();

            var tokenizedTextCorpusIds = allTokenizedTextCorpusIds.Distinct();
               // .Where(e => containedTokenizedTextCorpusIds.Contains(e.Id));

            var allCorpusIds = await Corpus.GetAllCorpusIdsAsync(context, cancellationToken);
            var containedCorpusIds =
                allTokenizedTextCorpusIds
                    .Select(e => e.CorpusId!.Id)
                    .Distinct();

            var corpusIds = allCorpusIds
                .Where(e => containedCorpusIds.Contains(e.Id));

            return new TopLevelProjectIds
            (
                translationSetIds,
                alignmentSetIds,
                parallelCorpusIds,
                tokenizedTextCorpusIds,
                corpusIds
            );
        }
    }
}
